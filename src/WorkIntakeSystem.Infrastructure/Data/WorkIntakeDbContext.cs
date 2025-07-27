using Microsoft.EntityFrameworkCore;
using WorkIntakeSystem.Core.Entities;

namespace WorkIntakeSystem.Infrastructure.Data;

public class WorkIntakeDbContext : DbContext
{
    public WorkIntakeDbContext(DbContextOptions<WorkIntakeDbContext> options) : base(options)
    {
    }

    // DbSets for all entities
    public DbSet<BusinessVertical> BusinessVerticals { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<WorkRequest> WorkRequests { get; set; }
    public DbSet<Priority> Priorities { get; set; }
    public DbSet<BusinessCapability> BusinessCapabilities { get; set; }
    public DbSet<CapabilityDepartmentMapping> CapabilityDepartmentMappings { get; set; }
    public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
    public DbSet<EventStore> EventStore { get; set; }
    public DbSet<AuditTrail> AuditTrails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entity relationships and constraints
        ConfigureBusinessVertical(modelBuilder);
        ConfigureDepartment(modelBuilder);
        ConfigureUser(modelBuilder);
        ConfigureWorkRequest(modelBuilder);
        ConfigurePriority(modelBuilder);
        ConfigureBusinessCapability(modelBuilder);
        ConfigureCapabilityDepartmentMapping(modelBuilder);
        ConfigureSystemConfiguration(modelBuilder);
        ConfigureEventStore(modelBuilder);
        ConfigureAuditTrail(modelBuilder);
        
        // Seed initial data
        SeedInitialData(modelBuilder);
    }

    private void ConfigureBusinessVertical(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BusinessVertical>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Configuration).HasDefaultValue("{}");
            entity.Property(e => e.ConfigurationHistory).HasDefaultValue("[]");
        });
    }

    private void ConfigureDepartment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.VotingWeight).HasColumnType("decimal(5,2)").HasDefaultValue(1.0m);
            entity.Property(e => e.CurrentUtilization).HasColumnType("decimal(5,2)").HasDefaultValue(0.0m);
            entity.Property(e => e.SkillMatrix).HasDefaultValue("{}");
            
            entity.HasOne(d => d.BusinessVertical)
                  .WithMany(bv => bv.Departments)
                  .HasForeignKey(d => d.BusinessVerticalId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CurrentWorkload).HasColumnType("decimal(5,2)").HasDefaultValue(0.0m);
            entity.Property(e => e.SkillSet).HasDefaultValue("{}");
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.HasOne(u => u.Department)
                  .WithMany(d => d.Users)
                  .HasForeignKey(u => u.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(u => u.BusinessVertical)
                  .WithMany()
                  .HasForeignKey(u => u.BusinessVerticalId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureWorkRequest(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Priority).HasColumnType("decimal(3,2)").HasDefaultValue(0.0m);
            entity.Property(e => e.BusinessValue).HasColumnType("decimal(3,2)").HasDefaultValue(0.5m);
            entity.Property(e => e.TimeDecayFactor).HasColumnType("decimal(3,2)").HasDefaultValue(1.0m);
            entity.Property(e => e.CapacityAdjustment).HasColumnType("decimal(3,2)").HasDefaultValue(1.0m);
            
            entity.HasOne(wr => wr.BusinessVertical)
                  .WithMany(bv => bv.WorkRequests)
                  .HasForeignKey(wr => wr.BusinessVerticalId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(wr => wr.Department)
                  .WithMany()
                  .HasForeignKey(wr => wr.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(wr => wr.Submitter)
                  .WithMany(u => u.SubmittedRequests)
                  .HasForeignKey(wr => wr.SubmitterId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(wr => wr.Capability)
                  .WithMany(bc => bc.WorkRequests)
                  .HasForeignKey(wr => wr.CapabilityId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigurePriority(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Priority>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Weight).HasColumnType("decimal(3,2)").HasDefaultValue(1.0m);
            entity.Property(e => e.BusinessValueScore).HasColumnType("decimal(3,2)").HasDefaultValue(0.5m);
            entity.Property(e => e.StrategicAlignment).HasColumnType("decimal(3,2)").HasDefaultValue(0.5m);
            
            // Composite unique constraint - one vote per department per work request
            entity.HasIndex(e => new { e.WorkRequestId, e.DepartmentId }).IsUnique();
            
            entity.HasOne(p => p.WorkRequest)
                  .WithMany(wr => wr.PriorityVotes)
                  .HasForeignKey(p => p.WorkRequestId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(p => p.Department)
                  .WithMany(d => d.Priorities)
                  .HasForeignKey(p => p.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(p => p.VotedBy)
                  .WithMany(u => u.PriorityVotes)
                  .HasForeignKey(p => p.VotedById)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureBusinessCapability(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BusinessCapability>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Configuration).HasDefaultValue("{}");
            entity.Property(e => e.DependencyMap).HasDefaultValue("{}");
            entity.Property(e => e.ResourceRequirements).HasDefaultValue("{}");
            
            entity.HasOne(bc => bc.BusinessVertical)
                  .WithMany(bv => bv.BusinessCapabilities)
                  .HasForeignKey(bc => bc.BusinessVerticalId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureCapabilityDepartmentMapping(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CapabilityDepartmentMapping>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Composite unique constraint
            entity.HasIndex(e => new { e.CapabilityId, e.DepartmentId }).IsUnique();
            
            entity.HasOne(cdm => cdm.Capability)
                  .WithMany(bc => bc.DepartmentMappings)
                  .HasForeignKey(cdm => cdm.CapabilityId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(cdm => cdm.Department)
                  .WithMany(d => d.CapabilityMappings)
                  .HasForeignKey(cdm => cdm.DepartmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureSystemConfiguration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConfigurationKey).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => new { e.ConfigurationKey, e.BusinessVerticalId, e.Version }).IsUnique();
            
            entity.HasOne(sc => sc.BusinessVertical)
                  .WithMany()
                  .HasForeignKey(sc => sc.BusinessVerticalId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(sc => sc.PreviousVersion)
                  .WithMany(sc => sc.NextVersions)
                  .HasForeignKey(sc => sc.PreviousVersionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureEventStore(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventStore>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AggregateId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Metadata).HasDefaultValue("{}");
            entity.HasIndex(e => new { e.AggregateId, e.EventVersion });
        });
    }

    private void ConfigureAuditTrail(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditTrail>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SecurityContext).HasDefaultValue("{}");
            
            entity.HasOne(at => at.WorkRequest)
                  .WithMany(wr => wr.AuditTrails)
                  .HasForeignKey(at => at.WorkRequestId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(at => at.ChangedBy)
                  .WithMany(u => u.AuditTrails)
                  .HasForeignKey(at => at.ChangedById)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void SeedInitialData(ModelBuilder modelBuilder)
    {
        // Seed default business vertical (Medicaid)
        modelBuilder.Entity<BusinessVertical>().HasData(
            new BusinessVertical
            {
                Id = 1,
                Name = "Medicaid",
                Description = "Medicaid business vertical",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                CreatedBy = "System",
                ModifiedBy = "System"
            }
        );

        // Seed default Medicaid departments
        var medicaidDepartments = new[]
        {
            new { Id = 1, Name = "Regulatory", Order = 1 },
            new { Id = 2, Name = "Compliance", Order = 2 },
            new { Id = 3, Name = "Communication", Order = 3 },
            new { Id = 4, Name = "Community Outreach", Order = 4 },
            new { Id = 5, Name = "Clinical Services", Order = 5 },
            new { Id = 6, Name = "Contract Performance", Order = 6 },
            new { Id = 7, Name = "Operations", Order = 7 },
            new { Id = 8, Name = "Provider Network Operations", Order = 8 },
            new { Id = 9, Name = "Provider Network Management", Order = 9 },
            new { Id = 10, Name = "Service Coordination", Order = 10 },
            new { Id = 11, Name = "Data and Technical Services", Order = 11 },
            new { Id = 12, Name = "Associate Relations", Order = 12 },
            new { Id = 13, Name = "Finance and Actuarial", Order = 13 },
            new { Id = 14, Name = "Human Resources", Order = 14 },
            new { Id = 15, Name = "Program Management and Quality", Order = 15 },
            new { Id = 16, Name = "Quality", Order = 16 },
            new { Id = 17, Name = "Population Health Medical Services", Order = 17 }
        };

        foreach (var dept in medicaidDepartments)
        {
            modelBuilder.Entity<Department>().HasData(
                new Department
                {
                    Id = dept.Id,
                    Name = dept.Name,
                    BusinessVerticalId = 1,
                    DisplayOrder = dept.Order,
                    DepartmentCode = dept.Name.Substring(0, Math.Min(3, dept.Name.Length)).ToUpper(),
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ModifiedBy = "System"
                }
            );
        }
    }
}