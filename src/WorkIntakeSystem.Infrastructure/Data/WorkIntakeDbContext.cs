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
    public DbSet<WorkCategoryConfiguration> WorkCategoryConfigurations { get; set; }
    public DbSet<ConfigurationChangeRequest> ConfigurationChangeRequests { get; set; }
    public DbSet<WorkflowStageConfiguration> WorkflowStages { get; set; }
    public DbSet<WorkflowTransition> WorkflowTransitions { get; set; }

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
        ConfigureWorkCategoryConfiguration(modelBuilder);
        ConfigureConfigurationChangeRequest(modelBuilder);
        ConfigureWorkflowStageConfiguration(modelBuilder);
        ConfigureWorkflowTransition(modelBuilder);
        
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
            
            // Performance indexes for common queries
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_WorkRequests_Status");
            entity.HasIndex(e => e.Category).HasDatabaseName("IX_WorkRequests_Category");
            entity.HasIndex(e => e.Priority).HasDatabaseName("IX_WorkRequests_Priority");
            entity.HasIndex(e => e.SubmitterId).HasDatabaseName("IX_WorkRequests_SubmitterId");
            entity.HasIndex(e => e.DepartmentId).HasDatabaseName("IX_WorkRequests_DepartmentId");
            entity.HasIndex(e => e.BusinessVerticalId).HasDatabaseName("IX_WorkRequests_BusinessVerticalId");
            entity.HasIndex(e => e.CapabilityId).HasDatabaseName("IX_WorkRequests_CapabilityId");
            entity.HasIndex(e => e.CreatedDate).HasDatabaseName("IX_WorkRequests_CreatedDate");
            entity.HasIndex(e => e.ModifiedDate).HasDatabaseName("IX_WorkRequests_ModifiedDate");
            
            // Composite indexes for analytics and reporting
            entity.HasIndex(e => new { e.Status, e.Category, e.Priority }).HasDatabaseName("IX_WorkRequests_StatusCategoryPriority");
            entity.HasIndex(e => new { e.DepartmentId, e.Status, e.CreatedDate }).HasDatabaseName("IX_WorkRequests_DepartmentStatusCreated");
            entity.HasIndex(e => new { e.BusinessVerticalId, e.Status, e.Priority }).HasDatabaseName("IX_WorkRequests_VerticalStatusPriority");
            entity.HasIndex(e => new { e.SubmitterId, e.Status, e.CreatedDate }).HasDatabaseName("IX_WorkRequests_SubmitterStatusCreated");
            
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
            
            // Performance indexes for analytics
            entity.HasIndex(e => e.WorkRequestId).HasDatabaseName("IX_Priorities_WorkRequestId");
            entity.HasIndex(e => e.DepartmentId).HasDatabaseName("IX_Priorities_DepartmentId");
            entity.HasIndex(e => e.VotedById).HasDatabaseName("IX_Priorities_VotedById");
            entity.HasIndex(e => e.VotedDate).HasDatabaseName("IX_Priorities_VotedDate");
            entity.HasIndex(e => e.Weight).HasDatabaseName("IX_Priorities_Weight");
            
            // Composite indexes for priority analysis
            entity.HasIndex(e => new { e.WorkRequestId, e.Weight }).HasDatabaseName("IX_Priorities_WorkRequestWeight");
            entity.HasIndex(e => new { e.DepartmentId, e.VotedDate }).HasDatabaseName("IX_Priorities_DepartmentVotedDate");
            
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
            
            // Performance indexes for audit trail queries
            entity.HasIndex(e => e.WorkRequestId).HasDatabaseName("IX_AuditTrails_WorkRequestId");
            entity.HasIndex(e => e.ChangedById).HasDatabaseName("IX_AuditTrails_ChangedById");
            entity.HasIndex(e => e.Action).HasDatabaseName("IX_AuditTrails_Action");
            entity.HasIndex(e => e.ChangedDate).HasDatabaseName("IX_AuditTrails_ChangedDate");
            
            // Composite indexes for audit analysis
            entity.HasIndex(e => new { e.WorkRequestId, e.ChangedDate }).HasDatabaseName("IX_AuditTrails_WorkRequestChangedDate");
            entity.HasIndex(e => new { e.ChangedById, e.ChangedDate }).HasDatabaseName("IX_AuditTrails_ChangedByChangedDate");
            entity.HasIndex(e => new { e.Action, e.ChangedDate }).HasDatabaseName("IX_AuditTrails_ActionChangedDate");
            
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

    private void ConfigureWorkCategoryConfiguration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkCategoryConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.RequiredFields).HasDefaultValue("{}");
            entity.Property(e => e.ApprovalMatrix).HasDefaultValue("{}");
            entity.Property(e => e.ValidationRules).HasDefaultValue("{}");
            entity.Property(e => e.NotificationTemplates).HasDefaultValue("{}");
            entity.Property(e => e.CustomFields).HasDefaultValue("{}");
            
            entity.HasOne(wcc => wcc.BusinessVertical)
                  .WithMany()
                  .HasForeignKey(wcc => wcc.BusinessVerticalId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureConfigurationChangeRequest(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConfigurationChangeRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConfigurationId).IsRequired();
            entity.Property(e => e.RequestedValue).IsRequired();
            entity.Property(e => e.ChangeReason).IsRequired();
            entity.Property(e => e.RequestedById).IsRequired();
            entity.Property(e => e.RequestedDate).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ApprovedDate);
            entity.Property(e => e.RejectedReason);
            entity.Property(e => e.ImplementedDate);
            entity.Property(e => e.RollbackDate);
            entity.Property(e => e.ImplementationNotes);

            // Performance indexes for change request management
            entity.HasIndex(e => e.ConfigurationId).HasDatabaseName("IX_ConfigurationChangeRequests_ConfigurationId");
            entity.HasIndex(e => e.RequestedById).HasDatabaseName("IX_ConfigurationChangeRequests_RequestedById");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_ConfigurationChangeRequests_Status");
            entity.HasIndex(e => e.RequestedDate).HasDatabaseName("IX_ConfigurationChangeRequests_RequestedDate");
            entity.HasIndex(e => e.ApprovedDate).HasDatabaseName("IX_ConfigurationChangeRequests_ApprovedDate");
            entity.HasIndex(e => e.ImplementedDate).HasDatabaseName("IX_ConfigurationChangeRequests_ImplementedDate");
            
            // Composite indexes for workflow and reporting
            entity.HasIndex(e => new { e.Status, e.RequestedDate }).HasDatabaseName("IX_ConfigurationChangeRequests_StatusRequestedDate");
            entity.HasIndex(e => new { e.ConfigurationId, e.Status }).HasDatabaseName("IX_ConfigurationChangeRequests_ConfigurationStatus");
            entity.HasIndex(e => new { e.RequestedById, e.Status }).HasDatabaseName("IX_ConfigurationChangeRequests_RequestedByStatus");

            entity.HasOne(ccr => ccr.Configuration)
                  .WithMany()
                  .HasForeignKey(ccr => ccr.ConfigurationId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(ccr => ccr.RequestedBy)
                  .WithMany()
                  .HasForeignKey(ccr => ccr.RequestedById)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(ccr => ccr.ApprovedBy)
                  .WithMany()
                  .HasForeignKey(ccr => ccr.ApprovedById)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureWorkflowStageConfiguration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowStageConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Order).IsRequired();
            entity.Property(e => e.Description).IsRequired();

            entity.HasIndex(e => new { e.BusinessVerticalId, e.Order }).HasDatabaseName("IX_WorkflowStages_VerticalOrder");

            entity.HasOne(ws => ws.BusinessVertical)
                  .WithMany()
                  .HasForeignKey(ws => ws.BusinessVerticalId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureWorkflowTransition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowTransition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransitionName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => new { e.FromStageId, e.ToStageId, e.BusinessVerticalId }).IsUnique();

            entity.HasOne(t => t.FromStage)
                  .WithMany(ws => ws.FromTransitions)
                  .HasForeignKey(t => t.FromStageId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.ToStage)
                  .WithMany(ws => ws.ToTransitions)
                  .HasForeignKey(t => t.ToStageId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.BusinessVertical)
                  .WithMany()
                  .HasForeignKey(t => t.BusinessVerticalId)
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