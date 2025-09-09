using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WorkIntakeSystem.Infrastructure.Data;

public class WorkIntakeDbContextFactory : IDesignTimeDbContextFactory<WorkIntakeDbContext>
{
    public WorkIntakeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WorkIntakeDbContext>();
        
        // Use a default connection string for design-time
        // This will be overridden by the actual configuration at runtime
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=WorkIntakeSystemDb;Trusted_Connection=true;MultipleActiveResultSets=true");
        
        return new WorkIntakeDbContext(optionsBuilder.Options);
    }
}
