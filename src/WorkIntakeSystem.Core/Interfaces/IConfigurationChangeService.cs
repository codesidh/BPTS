using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Core.Interfaces;

public interface IConfigurationChangeService
{
    Task<ConfigurationChangeRequest> CreateChangeRequestAsync(ConfigurationChangeRequest request);
    Task<ConfigurationChangeRequest?> GetChangeRequestAsync(int id);
    Task<IEnumerable<ConfigurationChangeRequest>> GetPendingChangeRequestsAsync();
    Task<IEnumerable<ConfigurationChangeRequest>> GetChangeRequestsByConfigurationAsync(int configurationId);
    Task<bool> ApproveChangeRequestAsync(int requestId, int approvedById, string? notes = null);
    Task<bool> RejectChangeRequestAsync(int requestId, int rejectedById, string reason);
    Task<bool> ImplementChangeRequestAsync(int requestId, int implementedById, string? notes = null);
    Task<bool> RollbackChangeRequestAsync(int requestId, int rolledBackById, string? reason = null);
    Task<IEnumerable<ConfigurationChangeRequest>> GetChangeHistoryAsync(int configurationId, DateTime? fromDate = null, DateTime? toDate = null);
} 