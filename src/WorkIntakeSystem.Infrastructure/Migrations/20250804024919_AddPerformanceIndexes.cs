using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WorkIntakeSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessVerticals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Configuration = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ConfigurationHistory = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessVerticals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessCapabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BusinessVerticalId = table.Column<int>(type: "int", nullable: false),
                    TechnicalOwner = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Configuration = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    Version = table.Column<int>(type: "int", nullable: false),
                    DependencyMap = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    ResourceRequirements = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessCapabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessCapabilities_BusinessVerticals_BusinessVerticalId",
                        column: x => x.BusinessVerticalId,
                        principalTable: "BusinessVerticals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BusinessVerticalId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    DepartmentCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    VotingWeight = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 1.0m),
                    ResourceCapacity = table.Column<int>(type: "int", nullable: false),
                    CurrentUtilization = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0.0m),
                    SkillMatrix = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_BusinessVerticals_BusinessVerticalId",
                        column: x => x.BusinessVerticalId,
                        principalTable: "BusinessVerticals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigurationKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfigurationValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BusinessVerticalId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChangeReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PreviousVersionId = table.Column<int>(type: "int", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemConfigurations_BusinessVerticals_BusinessVerticalId",
                        column: x => x.BusinessVerticalId,
                        principalTable: "BusinessVerticals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemConfigurations_SystemConfigurations_PreviousVersionId",
                        column: x => x.PreviousVersionId,
                        principalTable: "SystemConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkCategoryConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessVerticalId = table.Column<int>(type: "int", nullable: false),
                    WorkflowTemplateId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RequiredFields = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    ApprovalMatrix = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    SLAHours = table.Column<int>(type: "int", nullable: true),
                    ValidationRules = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    NotificationTemplates = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CustomFields = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkCategoryConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkCategoryConfigurations_BusinessVerticals_BusinessVerticalId",
                        column: x => x.BusinessVerticalId,
                        principalTable: "BusinessVerticals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CapabilityDepartmentMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CapabilityId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    AccessLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CanCreate = table.Column<bool>(type: "bit", nullable: false),
                    CanModify = table.Column<bool>(type: "bit", nullable: false),
                    CanView = table.Column<bool>(type: "bit", nullable: false),
                    CanApprove = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapabilityDepartmentMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapabilityDepartmentMappings_BusinessCapabilities_CapabilityId",
                        column: x => x.CapabilityId,
                        principalTable: "BusinessCapabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CapabilityDepartmentMappings_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PasswordSalt = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    BusinessVerticalId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    SkillSet = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    CurrentWorkload = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0.0m),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_BusinessVerticals_BusinessVerticalId",
                        column: x => x.BusinessVerticalId,
                        principalTable: "BusinessVerticals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationChangeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigurationId = table.Column<int>(type: "int", nullable: false),
                    RequestedValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangeReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedById = table.Column<int>(type: "int", nullable: false),
                    RequestedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApprovedById = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImplementedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RollbackDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImplementationNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationChangeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigurationChangeRequests_SystemConfigurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "SystemConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfigurationChangeRequests_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConfigurationChangeRequests_Users_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    BusinessVerticalId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    SubmitterId = table.Column<int>(type: "int", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentStage = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 0.0m),
                    CapabilityId = table.Column<int>(type: "int", nullable: true),
                    EstimatedEffort = table.Column<int>(type: "int", nullable: false),
                    ActualEffort = table.Column<int>(type: "int", nullable: false),
                    BusinessValue = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 0.5m),
                    TimeDecayFactor = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 1.0m),
                    CapacityAdjustment = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 1.0m),
                    PriorityLevel = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkRequests_BusinessCapabilities_CapabilityId",
                        column: x => x.CapabilityId,
                        principalTable: "BusinessCapabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkRequests_BusinessVerticals_BusinessVerticalId",
                        column: x => x.BusinessVerticalId,
                        principalTable: "BusinessVerticals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkRequests_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkRequests_Users_SubmitterId",
                        column: x => x.SubmitterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditTrails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkRequestId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedById = table.Column<int>(type: "int", nullable: false),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SecurityContext = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTrails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditTrails_Users_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditTrails_WorkRequests_WorkRequestId",
                        column: x => x.WorkRequestId,
                        principalTable: "WorkRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventStore",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AggregateId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventVersion = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CausationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    WorkRequestId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventStore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventStore_WorkRequests_WorkRequestId",
                        column: x => x.WorkRequestId,
                        principalTable: "WorkRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Priorities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkRequestId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Vote = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 1.0m),
                    VotedById = table.Column<int>(type: "int", nullable: false),
                    VotedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BusinessValueScore = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 0.5m),
                    StrategicAlignment = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 0.5m),
                    ResourceImpactAssessment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Priorities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Priorities_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Priorities_Users_VotedById",
                        column: x => x.VotedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Priorities_WorkRequests_WorkRequestId",
                        column: x => x.WorkRequestId,
                        principalTable: "WorkRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BusinessVerticals",
                columns: new[] { "Id", "Configuration", "ConfigurationHistory", "CreatedBy", "CreatedDate", "Description", "IsActive", "ModifiedBy", "ModifiedDate", "Name", "Version" },
                values: new object[] { 1, "{}", "[]", "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5155), "Medicaid business vertical", true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5157), "Medicaid", 1 });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "BusinessVerticalId", "CreatedBy", "CreatedDate", "DepartmentCode", "Description", "DisplayOrder", "IsActive", "ModifiedBy", "ModifiedDate", "Name", "ResourceCapacity", "SkillMatrix", "VotingWeight" },
                values: new object[,]
                {
                    { 1, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5374), "REG", "", 1, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5375), "Regulatory", 100, "{}", 1.0m },
                    { 2, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5399), "COM", "", 2, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5399), "Compliance", 100, "{}", 1.0m },
                    { 3, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5417), "COM", "", 3, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5417), "Communication", 100, "{}", 1.0m },
                    { 4, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5434), "COM", "", 4, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5434), "Community Outreach", 100, "{}", 1.0m },
                    { 5, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5451), "CLI", "", 5, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5451), "Clinical Services", 100, "{}", 1.0m },
                    { 6, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5471), "CON", "", 6, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5471), "Contract Performance", 100, "{}", 1.0m },
                    { 7, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5487), "OPE", "", 7, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5487), "Operations", 100, "{}", 1.0m },
                    { 8, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5503), "PRO", "", 8, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5503), "Provider Network Operations", 100, "{}", 1.0m },
                    { 9, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5519), "PRO", "", 9, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5520), "Provider Network Management", 100, "{}", 1.0m },
                    { 10, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5538), "SER", "", 10, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5539), "Service Coordination", 100, "{}", 1.0m },
                    { 11, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5555), "DAT", "", 11, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5555), "Data and Technical Services", 100, "{}", 1.0m },
                    { 12, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5588), "ASS", "", 12, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5588), "Associate Relations", 100, "{}", 1.0m },
                    { 13, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5606), "FIN", "", 13, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5606), "Finance and Actuarial", 100, "{}", 1.0m },
                    { 14, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5677), "HUM", "", 14, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5677), "Human Resources", 100, "{}", 1.0m },
                    { 15, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5697), "PRO", "", 15, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5697), "Program Management and Quality", 100, "{}", 1.0m },
                    { 16, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5714), "QUA", "", 16, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5715), "Quality", 100, "{}", 1.0m },
                    { 17, 1, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5730), "POP", "", 17, true, "System", new DateTime(2025, 8, 4, 2, 49, 17, 674, DateTimeKind.Utc).AddTicks(5731), "Population Health Medical Services", 100, "{}", 1.0m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_Action",
                table: "AuditTrails",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_ActionChangedDate",
                table: "AuditTrails",
                columns: new[] { "Action", "ChangedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_ChangedByChangedDate",
                table: "AuditTrails",
                columns: new[] { "ChangedById", "ChangedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_ChangedById",
                table: "AuditTrails",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_ChangedDate",
                table: "AuditTrails",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_WorkRequestChangedDate",
                table: "AuditTrails",
                columns: new[] { "WorkRequestId", "ChangedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_WorkRequestId",
                table: "AuditTrails",
                column: "WorkRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessCapabilities_BusinessVerticalId",
                table: "BusinessCapabilities",
                column: "BusinessVerticalId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessVerticals_Name",
                table: "BusinessVerticals",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CapabilityDepartmentMappings_CapabilityId_DepartmentId",
                table: "CapabilityDepartmentMappings",
                columns: new[] { "CapabilityId", "DepartmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CapabilityDepartmentMappings_DepartmentId",
                table: "CapabilityDepartmentMappings",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationChangeRequests_ApprovedById",
                table: "ConfigurationChangeRequests",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationChangeRequests_ApprovedDate",
                table: "ConfigurationChangeRequests",
                column: "ApprovedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationChangeRequests_ConfigurationId",
                table: "ConfigurationChangeRequests",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationChangeRequests_ConfigurationStatus",
                table: "ConfigurationChangeRequests",
                columns: new[] { "ConfigurationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationChangeRequests_ImplementedDate",
                table: "ConfigurationChangeRequests",
                column: "ImplementedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationChangeRequests_RequestedById",
                table: "ConfigurationChangeRequests",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationChangeRequests_RequestedByStatus",
                table: "ConfigurationChangeRequests",
                columns: new[] { "RequestedById", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationChangeRequests_RequestedDate",
                table: "ConfigurationChangeRequests",
                column: "RequestedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationChangeRequests_Status",
                table: "ConfigurationChangeRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationChangeRequests_StatusRequestedDate",
                table: "ConfigurationChangeRequests",
                columns: new[] { "Status", "RequestedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_BusinessVerticalId",
                table: "Departments",
                column: "BusinessVerticalId");

            migrationBuilder.CreateIndex(
                name: "IX_EventStore_AggregateId_EventVersion",
                table: "EventStore",
                columns: new[] { "AggregateId", "EventVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_EventStore_WorkRequestId",
                table: "EventStore",
                column: "WorkRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_DepartmentId",
                table: "Priorities",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_DepartmentVotedDate",
                table: "Priorities",
                columns: new[] { "DepartmentId", "VotedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_VotedById",
                table: "Priorities",
                column: "VotedById");

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_VotedDate",
                table: "Priorities",
                column: "VotedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_Weight",
                table: "Priorities",
                column: "Weight");

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_WorkRequestId",
                table: "Priorities",
                column: "WorkRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_WorkRequestId_DepartmentId",
                table: "Priorities",
                columns: new[] { "WorkRequestId", "DepartmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_WorkRequestWeight",
                table: "Priorities",
                columns: new[] { "WorkRequestId", "Weight" });

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfigurations_BusinessVerticalId",
                table: "SystemConfigurations",
                column: "BusinessVerticalId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfigurations_ConfigurationKey_BusinessVerticalId_Version",
                table: "SystemConfigurations",
                columns: new[] { "ConfigurationKey", "BusinessVerticalId", "Version" },
                unique: true,
                filter: "[BusinessVerticalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfigurations_PreviousVersionId",
                table: "SystemConfigurations",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_BusinessVerticalId",
                table: "Users",
                column: "BusinessVerticalId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkCategoryConfigurations_BusinessVerticalId",
                table: "WorkCategoryConfigurations",
                column: "BusinessVerticalId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_BusinessVerticalId",
                table: "WorkRequests",
                column: "BusinessVerticalId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_CapabilityId",
                table: "WorkRequests",
                column: "CapabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_Category",
                table: "WorkRequests",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_CreatedDate",
                table: "WorkRequests",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_DepartmentId",
                table: "WorkRequests",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_DepartmentStatusCreated",
                table: "WorkRequests",
                columns: new[] { "DepartmentId", "Status", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_ModifiedDate",
                table: "WorkRequests",
                column: "ModifiedDate");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_Priority",
                table: "WorkRequests",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_Status",
                table: "WorkRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_StatusCategoryPriority",
                table: "WorkRequests",
                columns: new[] { "Status", "Category", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_SubmitterId",
                table: "WorkRequests",
                column: "SubmitterId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_SubmitterStatusCreated",
                table: "WorkRequests",
                columns: new[] { "SubmitterId", "Status", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_VerticalStatusPriority",
                table: "WorkRequests",
                columns: new[] { "BusinessVerticalId", "Status", "Priority" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditTrails");

            migrationBuilder.DropTable(
                name: "CapabilityDepartmentMappings");

            migrationBuilder.DropTable(
                name: "ConfigurationChangeRequests");

            migrationBuilder.DropTable(
                name: "EventStore");

            migrationBuilder.DropTable(
                name: "Priorities");

            migrationBuilder.DropTable(
                name: "WorkCategoryConfigurations");

            migrationBuilder.DropTable(
                name: "SystemConfigurations");

            migrationBuilder.DropTable(
                name: "WorkRequests");

            migrationBuilder.DropTable(
                name: "BusinessCapabilities");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "BusinessVerticals");
        }
    }
}
