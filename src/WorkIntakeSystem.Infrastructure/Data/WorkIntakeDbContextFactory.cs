using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WorkIntakeSystem.Infrastructure.Data;

public class WorkIntakeDbContextFactory : IDesignTimeDbContextFactory<WorkIntakeDbContext>
{
    public WorkIntakeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WorkIntakeDbContext>();
        
        // Use Docker SQL Server connection string for design-time
        // This will be overridden by the actual configuration at runtime
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=WorkIntakeSystem;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;MultipleActiveResultSets=true");
        
        return new WorkIntakeDbContext(optionsBuilder.Options);
    }
}
