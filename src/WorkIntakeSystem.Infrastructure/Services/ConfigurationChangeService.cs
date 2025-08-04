using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace WorkIntakeSystem.Infrastructure.Services;

public class ConfigurationChangeService : IConfigurationChangeService
{
    private readonly WorkIntakeDbContext _context;
    private readonly ILogger<ConfigurationChangeService> _logger;
    private readonly IConfigurationService _configurationService;

    public ConfigurationChangeService(
        WorkIntakeDbContext context,
        ILogger<ConfigurationChangeService> logger,
        IConfigurationService configurationService)
    {
        _context = context;
        _logger = logger;
        _configurationService = configurationService;
    }

    public async Task<ConfigurationChangeRequest?> GetChangeRequestAsync(int id)
    {
        return await _context.ConfigurationChangeRequests
            .Include(c => c.Configuration)
            .Include(c => c.RequestedBy)
            .Include(c => c.ApprovedBy)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<ConfigurationChangeRequest>> GetPendingChangeRequestsAsync()
    {
        return await _context.ConfigurationChangeRequests
            .Include(c => c.Configuration)
            .Include(c => c.RequestedBy)
            .Where(c => c.Status == ConfigurationChangeStatus.Pending.ToString())
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task<ConfigurationChangeRequest> CreateChangeRequestAsync(ConfigurationChangeRequest changeRequest)
    {
        changeRequest.CreatedDate = DateTime.UtcNow;
        changeRequest.ModifiedDate = DateTime.UtcNow;
        changeRequest.Status = ConfigurationChangeStatus.Pending.ToString();

        _context.ConfigurationChangeRequests.Add(changeRequest);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Configuration change request created: {ChangeId} for configuration {ConfigurationId}", 
            changeRequest.Id, changeRequest.ConfigurationId);

        return changeRequest;
    }



    public async Task<IEnumerable<ConfigurationChangeRequest>> GetChangeRequestsByConfigurationAsync(int configurationId)
    {
        return await _context.ConfigurationChangeRequests
            .Include(c => c.Configuration)
            .Include(c => c.RequestedBy)
            .Where(c => c.ConfigurationId == configurationId)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> ApproveChangeRequestAsync(int requestId, int approvedById, string? notes = null)
    {
        var changeRequest = await _context.ConfigurationChangeRequests.FindAsync(requestId);
        if (changeRequest == null)
        {
            return false;
        }

        if (changeRequest.Status != ConfigurationChangeStatus.Pending.ToString())
        {
            return false;
        }

        // Apply the configuration change
        await ApplyConfigurationChangeAsync(changeRequest);

        // Update the change request status
        changeRequest.Status = ConfigurationChangeStatus.Approved.ToString();
        changeRequest.ApprovedById = approvedById;
        changeRequest.ApprovedDate = DateTime.UtcNow;
        changeRequest.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Configuration change request approved: {ChangeId} by {User}", requestId, approvedById);
        return true;
    }

    public async Task<bool> RejectChangeRequestAsync(int requestId, int rejectedById, string reason)
    {
        var changeRequest = await _context.ConfigurationChangeRequests.FindAsync(requestId);
        if (changeRequest == null)
        {
            return false;
        }

        if (changeRequest.Status != ConfigurationChangeStatus.Pending.ToString())
        {
            return false;
        }

        changeRequest.Status = ConfigurationChangeStatus.Rejected.ToString();
        changeRequest.RejectedReason = reason;
        changeRequest.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Configuration change request rejected: {ChangeId} by {User}", requestId, rejectedById);
        return true;
    }

    public async Task<bool> ImplementChangeRequestAsync(int requestId, int implementedById, string? notes = null)
    {
        var changeRequest = await _context.ConfigurationChangeRequests.FindAsync(requestId);
        if (changeRequest == null)
        {
            return false;
        }

        if (changeRequest.Status != ConfigurationChangeStatus.Approved.ToString())
        {
            return false;
        }

        // Apply the configuration change
        await ApplyConfigurationChangeAsync(changeRequest);

        changeRequest.Status = ConfigurationChangeStatus.Implemented.ToString();
        changeRequest.ImplementedDate = DateTime.UtcNow;
        changeRequest.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Configuration change request implemented: {ChangeId} by {User}", requestId, implementedById);
        return true;
    }

    public async Task<bool> RollbackChangeRequestAsync(int requestId, int rolledBackById, string? reason = null)
    {
        var changeRequest = await _context.ConfigurationChangeRequests.FindAsync(requestId);
        if (changeRequest == null)
        {
            return false;
        }

        if (changeRequest.Status != ConfigurationChangeStatus.Approved.ToString() && changeRequest.Status != ConfigurationChangeStatus.Implemented.ToString())
        {
            return false;
        }

        // Rollback the configuration change
        await RollbackConfigurationChangeAsync(changeRequest);

        changeRequest.Status = ConfigurationChangeStatus.RolledBack.ToString();
        changeRequest.RollbackDate = DateTime.UtcNow;
        changeRequest.ModifiedDate = DateTime.UtcNow;

        _logger.LogInformation("Configuration change rolled back: {ChangeId} by {User}", requestId, rolledBackById);
        return true;
    }

    public async Task<IEnumerable<ConfigurationChangeRequest>> GetChangeHistoryAsync(int configurationId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.ConfigurationChangeRequests
            .Include(c => c.Configuration)
            .Include(c => c.RequestedBy)
            .Where(c => c.ConfigurationId == configurationId);

        if (fromDate.HasValue)
        {
            query = query.Where(c => c.CreatedDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(c => c.CreatedDate <= toDate.Value);
        }

        return await query.OrderByDescending(c => c.CreatedDate).ToListAsync();
    }

    private async Task ApplyConfigurationChangeAsync(ConfigurationChangeRequest changeRequest)
    {
        try
        {
            // Load the configuration to get its type
            var configuration = await _context.SystemConfigurations.FindAsync(changeRequest.ConfigurationId);
            if (configuration == null)
            {
                throw new InvalidOperationException($"Configuration with ID {changeRequest.ConfigurationId} not found");
            }

            // Apply the requested value to the configuration
            configuration.ConfigurationValue = changeRequest.RequestedValue;
            configuration.ModifiedDate = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Configuration change applied: {ChangeId} for configuration {ConfigurationId}", 
                changeRequest.Id, changeRequest.ConfigurationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying configuration change {ChangeId}", changeRequest.Id);
            throw;
        }
    }

    private async Task RollbackConfigurationChangeAsync(ConfigurationChangeRequest changeRequest)
    {
        try
        {
            // For rollback, we would need to store the previous value somewhere
            // For now, we'll just log that rollback was requested
            _logger.LogInformation("Rollback requested for configuration change {ChangeId}", changeRequest.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back configuration change {ChangeId}", changeRequest.Id);
            throw;
        }
    }


} 