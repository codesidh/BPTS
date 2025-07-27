using Microsoft.EntityFrameworkCore;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Services;
using WorkIntakeSystem.Infrastructure.Data;

namespace WorkIntakeSystem.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly WorkIntakeDbContext _context;

    public DepartmentRepository(WorkIntakeDbContext context)
    {
        _context = context;
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _context.Departments
            .Include(d => d.BusinessVertical)
            .Include(d => d.Users)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<Department>> GetAllAsync()
    {
        return await _context.Departments
            .Include(d => d.BusinessVertical)
            .Where(d => d.IsActive)
            .OrderBy(d => d.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> GetByBusinessVerticalAsync(int businessVerticalId)
    {
        return await _context.Departments
            .Include(d => d.BusinessVertical)
            .Where(d => d.BusinessVerticalId == businessVerticalId && d.IsActive)
            .OrderBy(d => d.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Department> CreateAsync(Department department)
    {
        department.CreatedDate = DateTime.UtcNow;
        department.ModifiedDate = DateTime.UtcNow;
        
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task UpdateAsync(Department department)
    {
        department.ModifiedDate = DateTime.UtcNow;
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var department = await GetByIdAsync(id);
        if (department != null)
        {
            department.IsActive = false;
            department.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Department?> GetByNameAsync(string name, int businessVerticalId)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d => d.Name == name && 
                                     d.BusinessVerticalId == businessVerticalId && 
                                     d.IsActive);
    }

    public async Task UpdateUtilizationAsync(int departmentId, decimal utilization)
    {
        var department = await GetByIdAsync(departmentId);
        if (department != null)
        {
            department.CurrentUtilization = utilization;
            department.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}