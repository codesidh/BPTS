using Microsoft.EntityFrameworkCore;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Data;

namespace WorkIntakeSystem.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly WorkIntakeDbContext _context;

    public UserRepository(WorkIntakeDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Department)
            .Include(u => u.BusinessVertical)
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Department)
            .Include(u => u.BusinessVertical)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Department)
            .Include(u => u.BusinessVertical)
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByDepartmentAsync(int departmentId)
    {
        return await _context.Users
            .Include(u => u.Department)
            .Include(u => u.BusinessVertical)
            .Where(u => u.DepartmentId == departmentId && u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByBusinessVerticalAsync(int businessVerticalId)
    {
        return await _context.Users
            .Include(u => u.Department)
            .Include(u => u.BusinessVertical)
            .Where(u => u.BusinessVerticalId == businessVerticalId && u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        user.CreatedDate = DateTime.UtcNow;
        user.ModifiedDate = DateTime.UtcNow;
        user.IsActive = true;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        user.ModifiedDate = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            user.IsActive = false;
            user.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email && u.IsActive);
    }

    public async Task<User?> GetByWindowsIdentityAsync(string windowsIdentity)
    {
        return await _context.Users
            .Include(u => u.Department)
            .Include(u => u.BusinessVertical)
            .FirstOrDefaultAsync(u => u.WindowsIdentity == windowsIdentity && u.IsActive);
    }

    public async Task<User> AddAsync(User user)
    {
        user.CreatedDate = DateTime.UtcNow;
        user.ModifiedDate = DateTime.UtcNow;
        user.IsActive = true;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
} 