using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WorkIntakeSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityMonitoringEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditTrails_BusinessVerticals_BusinessVerticalId",
                table: "AuditTrails");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditTrails_Users_UserId",
                table: "AuditTrails");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessCapabilities_BusinessVerticals_BusinessVerticalId",
                table: "BusinessCapabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_CapabilityDepartmentMappings_BusinessCapabilities_BusinessCapabilityId",
                table: "CapabilityDepartmentMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationChangeRequests_BusinessVerticals_BusinessVerticalId",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationChangeRequests_Users_UserId",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Priorities_Users_UserId",
                table: "Priorities");

            migrationBuilder.DropForeignKey(
                name: "FK_PriorityConfigurations_BusinessVerticals_BusinessVerticalId",
                table: "PriorityConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_BusinessVerticals_BusinessVerticalId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkCategoryConfigurations_BusinessVerticals_BusinessVerticalId",
                table: "WorkCategoryConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowStageConfigurations_BusinessVerticals_BusinessVerticalId",
                table: "WorkflowStageConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowTransitions_BusinessVerticals_BusinessVerticalId",
                table: "WorkflowTransitions");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequests_Users_UserId",
                table: "WorkRequests");

            migrationBuilder.DropTable(
                name: "EventSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_Users_ActiveDirectorySid",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_WindowsIdentity",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_PriorityConfigurations_BusinessVerticalId",
                table: "PriorityConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_CapabilityDepartmentMappings_BusinessCapabilityId",
                table: "CapabilityDepartmentMappings");

            migrationBuilder.DropIndex(
                name: "IX_AuditTrails_BusinessVerticalId",
                table: "AuditTrails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkflowStageConfigurations",
                table: "WorkflowStageConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowStageConfigurations_BusinessVerticalId",
                table: "WorkflowStageConfigurations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventStores",
                table: "EventStores");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "WorkCategory",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Priorities");

            migrationBuilder.DropColumn(
                name: "RequestType",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropColumn(
                name: "BusinessVerticalId",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "EntityName",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "EventStores");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "EventStores");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "EventStores");

            migrationBuilder.RenameTable(
                name: "WorkflowStageConfigurations",
                newName: "WorkflowStages");

            migrationBuilder.RenameTable(
                name: "EventStores",
                newName: "EventStore");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "WorkRequests",
                newName: "SubmitterId");

            migrationBuilder.RenameColumn(
                name: "EstimatedHours",
                table: "WorkRequests",
                newName: "PriorityLevel");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequests_UserId",
                table: "WorkRequests",
                newName: "IX_WorkRequests_SubmitterId");

            migrationBuilder.RenameColumn(
                name: "TargetStageId",
                table: "WorkflowTransitions",
                newName: "ToStageId");

            migrationBuilder.RenameColumn(
                name: "SourceStageId",
                table: "WorkflowTransitions",
                newName: "FromStageId");

            migrationBuilder.RenameColumn(
                name: "Settings",
                table: "WorkflowTransitions",
                newName: "ValidationRules");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "WorkflowTransitions",
                newName: "TransitionName");

            migrationBuilder.RenameColumn(
                name: "Settings",
                table: "WorkCategoryConfigurations",
                newName: "ModifiedBy");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "WorkCategoryConfigurations",
                newName: "CategoryName");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "SystemConfigurations",
                newName: "ModifiedBy");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "SystemConfigurations",
                newName: "ConfigurationKey");

            migrationBuilder.RenameColumn(
                name: "Settings",
                table: "PriorityConfigurations",
                newName: "NotificationSettings");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "PriorityConfigurations",
                newName: "PriorityName");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Priorities",
                newName: "VotedById");

            migrationBuilder.RenameColumn(
                name: "PriorityLevel",
                table: "Priorities",
                newName: "Vote");

            migrationBuilder.RenameIndex(
                name: "IX_Priorities_UserId",
                table: "Priorities",
                newName: "IX_Priorities_VotedById");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ConfigurationChangeRequests",
                newName: "RequestedById");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "ConfigurationChangeRequests",
                newName: "RequestedValue");

            migrationBuilder.RenameColumn(
                name: "BusinessVerticalId",
                table: "ConfigurationChangeRequests",
                newName: "ConfigurationId");

            migrationBuilder.RenameIndex(
                name: "IX_ConfigurationChangeRequests_UserId",
                table: "ConfigurationChangeRequests",
                newName: "IX_ConfigurationChangeRequests_RequestedById");

            migrationBuilder.RenameIndex(
                name: "IX_ConfigurationChangeRequests_BusinessVerticalId",
                table: "ConfigurationChangeRequests",
                newName: "IX_ConfigurationChangeRequests_ConfigurationId");

            migrationBuilder.RenameColumn(
                name: "BusinessCapabilityId",
                table: "CapabilityDepartmentMappings",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AuditTrails",
                newName: "WorkRequestId");

            migrationBuilder.RenameColumn(
                name: "ModifiedDate",
                table: "AuditTrails",
                newName: "ChangedDate");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "AuditTrails",
                newName: "ChangedById");

            migrationBuilder.RenameColumn(
                name: "Details",
                table: "AuditTrails",
                newName: "OldValue");

            migrationBuilder.RenameIndex(
                name: "IX_AuditTrails_UserId",
                table: "AuditTrails",
                newName: "IX_AuditTrails_WorkRequestId");

            migrationBuilder.RenameColumn(
                name: "Settings",
                table: "WorkflowStages",
                newName: "ValidationRules");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "WorkRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "Priority",
                table: "WorkRequests",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0.0m,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "BusinessValue",
                table: "WorkRequests",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0.5m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualDate",
                table: "WorkRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActualEffort",
                table: "WorkRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BusinessVerticalId",
                table: "WorkRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CapabilityId",
                table: "WorkRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CapacityAdjustment",
                table: "WorkRequests",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 1.0m);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "WorkRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "WorkRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CurrentStage",
                table: "WorkRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "WorkRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedEffort",
                table: "WorkRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "WorkRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TargetDate",
                table: "WorkRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TimeDecayFactor",
                table: "WorkRequests",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 1.0m);

            migrationBuilder.AlterColumn<int>(
                name: "BusinessVerticalId",
                table: "WorkflowTransitions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AutoTransitionDelayMinutes",
                table: "WorkflowTransitions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConditionScript",
                table: "WorkflowTransitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "WorkflowTransitions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "WorkflowTransitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EventSourceId",
                table: "WorkflowTransitions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "WorkflowTransitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "NotificationRequired",
                table: "WorkflowTransitions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NotificationTemplate",
                table: "WorkflowTransitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequiredRole",
                table: "WorkflowTransitions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WorkCategoryConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalMatrix",
                table: "WorkCategoryConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "WorkCategoryConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomFields",
                table: "WorkCategoryConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "WorkCategoryConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NotificationTemplates",
                table: "WorkCategoryConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "RequiredFields",
                table: "WorkCategoryConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<int>(
                name: "SLAHours",
                table: "WorkCategoryConfigurations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationRules",
                table: "WorkCategoryConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<int>(
                name: "WorkflowTemplateId",
                table: "WorkCategoryConfigurations",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SkillSet",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentWorkload",
                table: "Users",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0.0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SystemConfigurations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "SystemConfigurations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "SystemConfigurations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "BusinessVerticalId",
                table: "SystemConfigurations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChangeReason",
                table: "SystemConfigurations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConfigurationValue",
                table: "SystemConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SystemConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataType",
                table: "SystemConfigurations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "SystemConfigurations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "SystemConfigurations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEditable",
                table: "SystemConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PreviousVersionId",
                table: "SystemConfigurations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "SystemConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PriorityConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PriorityConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AutoAdjustmentRules",
                table: "PriorityConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessValueWeights",
                table: "PriorityConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "CapacityFactors",
                table: "PriorityConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "ColorCode",
                table: "PriorityConfigurations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PriorityConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EscalationRules",
                table: "PriorityConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EscalationThresholdHours",
                table: "PriorityConfigurations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconClass",
                table: "PriorityConfigurations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MaxScore",
                table: "PriorityConfigurations",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinScore",
                table: "PriorityConfigurations",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "PriorityConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SLAHours",
                table: "PriorityConfigurations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeDecayConfiguration",
                table: "PriorityConfigurations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<decimal>(
                name: "BusinessValueScore",
                table: "Priorities",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0.5m);

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "Priorities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Priorities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Priorities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Priorities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResourceImpactAssessment",
                table: "Priorities",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "StrategicAlignment",
                table: "Priorities",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0.5m);

            migrationBuilder.AddColumn<DateTime>(
                name: "VotedDate",
                table: "Priorities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "Priorities",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 1.0m);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Departments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessVerticalId",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentUtilization",
                table: "Departments",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0.0m);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentCode",
                table: "Departments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ResourceCapacity",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SkillMatrix",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<decimal>(
                name: "VotingWeight",
                table: "Departments",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 1.0m);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ConfigurationChangeRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedById",
                table: "ConfigurationChangeRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "ConfigurationChangeRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChangeReason",
                table: "ConfigurationChangeRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ConfigurationChangeRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImplementationNotes",
                table: "ConfigurationChangeRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImplementedDate",
                table: "ConfigurationChangeRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "ConfigurationChangeRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RejectedReason",
                table: "ConfigurationChangeRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedDate",
                table: "ConfigurationChangeRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RollbackDate",
                table: "ConfigurationChangeRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccessLevel",
                table: "CapabilityDepartmentMappings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "CanApprove",
                table: "CapabilityDepartmentMappings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanCreate",
                table: "CapabilityDepartmentMappings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanModify",
                table: "CapabilityDepartmentMappings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanView",
                table: "CapabilityDepartmentMappings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CapabilityId",
                table: "CapabilityDepartmentMappings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CapabilityDepartmentMappings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "CapabilityDepartmentMappings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "CapabilityDepartmentMappings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "CapabilityDepartmentMappings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BusinessVerticals",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Configuration",
                table: "BusinessVerticals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "ConfigurationHistory",
                table: "BusinessVerticals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "BusinessVerticals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "BusinessVerticals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "StrategicImportance",
                table: "BusinessVerticals",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "BusinessVerticals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BusinessCapabilities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "BusinessCapabilities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Configuration",
                table: "BusinessCapabilities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "BusinessCapabilities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DependencyMap",
                table: "BusinessCapabilities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "BusinessCapabilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "BusinessCapabilities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "BusinessCapabilities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResourceRequirements",
                table: "BusinessCapabilities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "SubCategory",
                table: "BusinessCapabilities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TechnicalOwner",
                table: "BusinessCapabilities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "BusinessCapabilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Metadata",
                table: "AuditTrails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "AuditTrails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "AuditTrails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "AuditTrails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IPAddress",
                table: "AuditTrails",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NewValue",
                table: "AuditTrails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecurityContext",
                table: "AuditTrails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "AuditTrails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "AuditTrails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WorkflowStages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BusinessVerticalId",
                table: "WorkflowStages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AllowedTransitions",
                table: "WorkflowStages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ApprovalRequired",
                table: "WorkflowStages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutoTransition",
                table: "WorkflowStages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ChangeHistory",
                table: "WorkflowStages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "WorkflowStages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "WorkflowStages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "WorkflowStages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NotificationTemplate",
                table: "WorkflowStages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequiredRoles",
                table: "WorkflowStages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SLAHours",
                table: "WorkflowStages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StageType",
                table: "WorkflowStages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "WorkflowStages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AggregateId",
                table: "EventStore",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CausationId",
                table: "EventStore",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "EventStore",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "EventStore",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EventVersion",
                table: "EventStore",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "EventStore",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<int>(
                name: "WorkRequestId",
                table: "EventStore",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkflowStages",
                table: "WorkflowStages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventStore",
                table: "EventStore",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ComplianceViolations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Framework = table.Column<int>(type: "int", nullable: false),
                    Requirement = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Evidence = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceViolations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceViolations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlertType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActionRequired = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsEscalated = table.Column<bool>(type: "bit", nullable: false),
                    EscalatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EscalatedTo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAlerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    RequestData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    Endpoint = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HttpMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    IsSuspicious = table.Column<bool>(type: "bit", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityEvents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityIncidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Impact = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AffectedResources = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IncidentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Medium"),
                    InvestigationNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityIncidents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityThreats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThreatType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    TargetResource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MitigationSteps = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityThreats", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "BusinessVerticals",
                columns: new[] { "Id", "Configuration", "ConfigurationHistory", "CreatedBy", "CreatedDate", "Description", "IsActive", "ModifiedBy", "ModifiedDate", "Name", "StrategicImportance", "Version" },
                values: new object[] { 1, "{}", "[]", "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(5859), "Medicaid business vertical", true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(5861), "Medicaid", 1.0m, 1 });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "BusinessVerticalId", "CreatedBy", "CreatedDate", "DepartmentCode", "Description", "DisplayOrder", "IsActive", "ModifiedBy", "ModifiedDate", "Name", "ResourceCapacity", "SkillMatrix", "VotingWeight" },
                values: new object[,]
                {
                    { 1, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6137), "REG", "", 1, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6137), "Regulatory", 100, "{}", 1.0m },
                    { 2, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6161), "COM", "", 2, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6161), "Compliance", 100, "{}", 1.0m },
                    { 3, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6180), "COM", "", 3, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6180), "Communication", 100, "{}", 1.0m },
                    { 4, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6197), "COM", "", 4, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6197), "Community Outreach", 100, "{}", 1.0m },
                    { 5, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6215), "CLI", "", 5, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6216), "Clinical Services", 100, "{}", 1.0m },
                    { 6, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6236), "CON", "", 6, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6237), "Contract Performance", 100, "{}", 1.0m },
                    { 7, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6254), "OPE", "", 7, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6255), "Operations", 100, "{}", 1.0m },
                    { 8, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6272), "PRO", "", 8, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6272), "Provider Network Operations", 100, "{}", 1.0m },
                    { 9, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6291), "PRO", "", 9, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6291), "Provider Network Management", 100, "{}", 1.0m },
                    { 10, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6310), "SER", "", 10, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6310), "Service Coordination", 100, "{}", 1.0m },
                    { 11, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6352), "DAT", "", 11, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6352), "Data and Technical Services", 100, "{}", 1.0m },
                    { 12, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6372), "ASS", "", 12, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6372), "Associate Relations", 100, "{}", 1.0m },
                    { 13, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6390), "FIN", "", 13, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6390), "Finance and Actuarial", 100, "{}", 1.0m },
                    { 14, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6407), "HUM", "", 14, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6408), "Human Resources", 100, "{}", 1.0m },
                    { 15, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6424), "PRO", "", 15, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6425), "Program Management and Quality", 100, "{}", 1.0m },
                    { 16, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6442), "QUA", "", 16, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6442), "Quality", 100, "{}", 1.0m },
                    { 17, 1, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6458), "POP", "", 17, true, "System", new DateTime(2025, 9, 9, 0, 38, 11, 582, DateTimeKind.Utc).AddTicks(6458), "Population Health Medical Services", 100, "{}", 1.0m }
                });

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
                name: "IX_WorkRequests_SubmitterStatusCreated",
                table: "WorkRequests",
                columns: new[] { "SubmitterId", "Status", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkRequests_VerticalStatusPriority",
                table: "WorkRequests",
                columns: new[] { "BusinessVerticalId", "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_FromStageId_ToStageId_BusinessVerticalId",
                table: "WorkflowTransitions",
                columns: new[] { "FromStageId", "ToStageId", "BusinessVerticalId" },
                unique: true,
                filter: "[BusinessVerticalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_ToStageId",
                table: "WorkflowTransitions",
                column: "ToStageId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

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
                name: "IX_PriorityConfigurations_BusinessVerticalId_PriorityName",
                table: "PriorityConfigurations",
                columns: new[] { "BusinessVerticalId", "PriorityName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_DepartmentId",
                table: "Priorities",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_DepartmentVotedDate",
                table: "Priorities",
                columns: new[] { "DepartmentId", "VotedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_VotedDate",
                table: "Priorities",
                column: "VotedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_Weight",
                table: "Priorities",
                column: "Weight");

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
                name: "IX_Departments_BusinessVerticalId",
                table: "Departments",
                column: "BusinessVerticalId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationChangeRequests_ApprovedById",
                table: "ConfigurationChangeRequests",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationChangeRequests_ApprovedDate",
                table: "ConfigurationChangeRequests",
                column: "ApprovedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationChangeRequests_ConfigurationStatus",
                table: "ConfigurationChangeRequests",
                columns: new[] { "ConfigurationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationChangeRequests_ImplementedDate",
                table: "ConfigurationChangeRequests",
                column: "ImplementedDate");

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
                name: "IX_CapabilityDepartmentMappings_CapabilityId_DepartmentId",
                table: "CapabilityDepartmentMappings",
                columns: new[] { "CapabilityId", "DepartmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessVerticals_Name",
                table: "BusinessVerticals",
                column: "Name",
                unique: true);

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
                name: "IX_WorkflowStages_VerticalOrder",
                table: "WorkflowStages",
                columns: new[] { "BusinessVerticalId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_EventStore_AggregateId_EventVersion",
                table: "EventStore",
                columns: new[] { "AggregateId", "EventVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_EventStore_WorkRequestId",
                table: "EventStore",
                column: "WorkRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceViolations_DetectedAt",
                table: "ComplianceViolations",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceViolations_Framework",
                table: "ComplianceViolations",
                column: "Framework");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceViolations_IsResolved",
                table: "ComplianceViolations",
                column: "IsResolved");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceViolations_UserId",
                table: "ComplianceViolations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAlerts_CreatedAt",
                table: "SecurityAlerts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAlerts_Severity",
                table: "SecurityAlerts",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAlerts_Status",
                table: "SecurityAlerts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_Action",
                table: "SecurityAuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_Timestamp",
                table: "SecurityAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_UserId",
                table: "SecurityAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_EventType",
                table: "SecurityEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_IsSuspicious",
                table: "SecurityEvents",
                column: "IsSuspicious");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_Timestamp",
                table: "SecurityEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_UserId",
                table: "SecurityEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_DetectedAt",
                table: "SecurityIncidents",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_Severity",
                table: "SecurityIncidents",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_Status",
                table: "SecurityIncidents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityThreats_DetectedAt",
                table: "SecurityThreats",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityThreats_Severity",
                table: "SecurityThreats",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityThreats_Status",
                table: "SecurityThreats",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditTrails_Users_ChangedById",
                table: "AuditTrails",
                column: "ChangedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditTrails_WorkRequests_WorkRequestId",
                table: "AuditTrails",
                column: "WorkRequestId",
                principalTable: "WorkRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessCapabilities_BusinessVerticals_BusinessVerticalId",
                table: "BusinessCapabilities",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CapabilityDepartmentMappings_BusinessCapabilities_CapabilityId",
                table: "CapabilityDepartmentMappings",
                column: "CapabilityId",
                principalTable: "BusinessCapabilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationChangeRequests_SystemConfigurations_ConfigurationId",
                table: "ConfigurationChangeRequests",
                column: "ConfigurationId",
                principalTable: "SystemConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationChangeRequests_Users_ApprovedById",
                table: "ConfigurationChangeRequests",
                column: "ApprovedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationChangeRequests_Users_RequestedById",
                table: "ConfigurationChangeRequests",
                column: "RequestedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_BusinessVerticals_BusinessVerticalId",
                table: "Departments",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventStore_WorkRequests_WorkRequestId",
                table: "EventStore",
                column: "WorkRequestId",
                principalTable: "WorkRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Priorities_Departments_DepartmentId",
                table: "Priorities",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Priorities_Users_VotedById",
                table: "Priorities",
                column: "VotedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PriorityConfigurations_BusinessVerticals_BusinessVerticalId",
                table: "PriorityConfigurations",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemConfigurations_BusinessVerticals_BusinessVerticalId",
                table: "SystemConfigurations",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemConfigurations_SystemConfigurations_PreviousVersionId",
                table: "SystemConfigurations",
                column: "PreviousVersionId",
                principalTable: "SystemConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_BusinessVerticals_BusinessVerticalId",
                table: "Users",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkCategoryConfigurations_BusinessVerticals_BusinessVerticalId",
                table: "WorkCategoryConfigurations",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowStages_BusinessVerticals_BusinessVerticalId",
                table: "WorkflowStages",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowTransitions_BusinessVerticals_BusinessVerticalId",
                table: "WorkflowTransitions",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowTransitions_WorkflowStages_FromStageId",
                table: "WorkflowTransitions",
                column: "FromStageId",
                principalTable: "WorkflowStages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowTransitions_WorkflowStages_ToStageId",
                table: "WorkflowTransitions",
                column: "ToStageId",
                principalTable: "WorkflowStages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequests_BusinessCapabilities_CapabilityId",
                table: "WorkRequests",
                column: "CapabilityId",
                principalTable: "BusinessCapabilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequests_BusinessVerticals_BusinessVerticalId",
                table: "WorkRequests",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequests_Departments_DepartmentId",
                table: "WorkRequests",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequests_Users_SubmitterId",
                table: "WorkRequests",
                column: "SubmitterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditTrails_Users_ChangedById",
                table: "AuditTrails");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditTrails_WorkRequests_WorkRequestId",
                table: "AuditTrails");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessCapabilities_BusinessVerticals_BusinessVerticalId",
                table: "BusinessCapabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_CapabilityDepartmentMappings_BusinessCapabilities_CapabilityId",
                table: "CapabilityDepartmentMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationChangeRequests_SystemConfigurations_ConfigurationId",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationChangeRequests_Users_ApprovedById",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfigurationChangeRequests_Users_RequestedById",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_BusinessVerticals_BusinessVerticalId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_EventStore_WorkRequests_WorkRequestId",
                table: "EventStore");

            migrationBuilder.DropForeignKey(
                name: "FK_Priorities_Departments_DepartmentId",
                table: "Priorities");

            migrationBuilder.DropForeignKey(
                name: "FK_Priorities_Users_VotedById",
                table: "Priorities");

            migrationBuilder.DropForeignKey(
                name: "FK_PriorityConfigurations_BusinessVerticals_BusinessVerticalId",
                table: "PriorityConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemConfigurations_BusinessVerticals_BusinessVerticalId",
                table: "SystemConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemConfigurations_SystemConfigurations_PreviousVersionId",
                table: "SystemConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_BusinessVerticals_BusinessVerticalId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkCategoryConfigurations_BusinessVerticals_BusinessVerticalId",
                table: "WorkCategoryConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowStages_BusinessVerticals_BusinessVerticalId",
                table: "WorkflowStages");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowTransitions_BusinessVerticals_BusinessVerticalId",
                table: "WorkflowTransitions");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowTransitions_WorkflowStages_FromStageId",
                table: "WorkflowTransitions");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowTransitions_WorkflowStages_ToStageId",
                table: "WorkflowTransitions");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequests_BusinessCapabilities_CapabilityId",
                table: "WorkRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequests_BusinessVerticals_BusinessVerticalId",
                table: "WorkRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequests_Departments_DepartmentId",
                table: "WorkRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRequests_Users_SubmitterId",
                table: "WorkRequests");

            migrationBuilder.DropTable(
                name: "ComplianceViolations");

            migrationBuilder.DropTable(
                name: "SecurityAlerts");

            migrationBuilder.DropTable(
                name: "SecurityAuditLogs");

            migrationBuilder.DropTable(
                name: "SecurityEvents");

            migrationBuilder.DropTable(
                name: "SecurityIncidents");

            migrationBuilder.DropTable(
                name: "SecurityThreats");

            migrationBuilder.DropIndex(
                name: "IX_WorkRequests_BusinessVerticalId",
                table: "WorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_WorkRequests_CapabilityId",
                table: "WorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_WorkRequests_Category",
                table: "WorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_WorkRequests_CreatedDate",
                table: "WorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_WorkRequests_DepartmentId",
                table: "WorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_WorkRequests_DepartmentStatusCreated",
                table: "WorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_WorkRequests_ModifiedDate",
                table: "WorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_WorkRequests_Priority",
                table: "WorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_WorkRequests_Status",
                table: "WorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_WorkRequests_StatusCategoryPriority",
                table: "WorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_WorkRequests_SubmitterStatusCreated",
                table: "WorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_WorkRequests_VerticalStatusPriority",
                table: "WorkRequests");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowTransitions_FromStageId_ToStageId_BusinessVerticalId",
                table: "WorkflowTransitions");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowTransitions_ToStageId",
                table: "WorkflowTransitions");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_SystemConfigurations_BusinessVerticalId",
                table: "SystemConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_SystemConfigurations_ConfigurationKey_BusinessVerticalId_Version",
                table: "SystemConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_SystemConfigurations_PreviousVersionId",
                table: "SystemConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_PriorityConfigurations_BusinessVerticalId_PriorityName",
                table: "PriorityConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_Priorities_DepartmentId",
                table: "Priorities");

            migrationBuilder.DropIndex(
                name: "IX_Priorities_DepartmentVotedDate",
                table: "Priorities");

            migrationBuilder.DropIndex(
                name: "IX_Priorities_VotedDate",
                table: "Priorities");

            migrationBuilder.DropIndex(
                name: "IX_Priorities_Weight",
                table: "Priorities");

            migrationBuilder.DropIndex(
                name: "IX_Priorities_WorkRequestId_DepartmentId",
                table: "Priorities");

            migrationBuilder.DropIndex(
                name: "IX_Priorities_WorkRequestWeight",
                table: "Priorities");

            migrationBuilder.DropIndex(
                name: "IX_Departments_BusinessVerticalId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_ConfigurationChangeRequests_ApprovedById",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropIndex(
                name: "IX_ConfigurationChangeRequests_ApprovedDate",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropIndex(
                name: "IX_ConfigurationChangeRequests_ConfigurationStatus",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropIndex(
                name: "IX_ConfigurationChangeRequests_ImplementedDate",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropIndex(
                name: "IX_ConfigurationChangeRequests_RequestedByStatus",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropIndex(
                name: "IX_ConfigurationChangeRequests_RequestedDate",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropIndex(
                name: "IX_ConfigurationChangeRequests_Status",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropIndex(
                name: "IX_ConfigurationChangeRequests_StatusRequestedDate",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropIndex(
                name: "IX_CapabilityDepartmentMappings_CapabilityId_DepartmentId",
                table: "CapabilityDepartmentMappings");

            migrationBuilder.DropIndex(
                name: "IX_BusinessVerticals_Name",
                table: "BusinessVerticals");

            migrationBuilder.DropIndex(
                name: "IX_AuditTrails_Action",
                table: "AuditTrails");

            migrationBuilder.DropIndex(
                name: "IX_AuditTrails_ActionChangedDate",
                table: "AuditTrails");

            migrationBuilder.DropIndex(
                name: "IX_AuditTrails_ChangedByChangedDate",
                table: "AuditTrails");

            migrationBuilder.DropIndex(
                name: "IX_AuditTrails_ChangedById",
                table: "AuditTrails");

            migrationBuilder.DropIndex(
                name: "IX_AuditTrails_ChangedDate",
                table: "AuditTrails");

            migrationBuilder.DropIndex(
                name: "IX_AuditTrails_WorkRequestChangedDate",
                table: "AuditTrails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkflowStages",
                table: "WorkflowStages");

            migrationBuilder.DropIndex(
                name: "IX_WorkflowStages_VerticalOrder",
                table: "WorkflowStages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventStore",
                table: "EventStore");

            migrationBuilder.DropIndex(
                name: "IX_EventStore_AggregateId_EventVersion",
                table: "EventStore");

            migrationBuilder.DropIndex(
                name: "IX_EventStore_WorkRequestId",
                table: "EventStore");

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "BusinessVerticals",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "ActualDate",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "ActualEffort",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "BusinessVerticalId",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "CapabilityId",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "CapacityAdjustment",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "CurrentStage",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "EstimatedEffort",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "TargetDate",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "TimeDecayFactor",
                table: "WorkRequests");

            migrationBuilder.DropColumn(
                name: "AutoTransitionDelayMinutes",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "ConditionScript",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "EventSourceId",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "NotificationRequired",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "NotificationTemplate",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "RequiredRole",
                table: "WorkflowTransitions");

            migrationBuilder.DropColumn(
                name: "ApprovalMatrix",
                table: "WorkCategoryConfigurations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "WorkCategoryConfigurations");

            migrationBuilder.DropColumn(
                name: "CustomFields",
                table: "WorkCategoryConfigurations");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "WorkCategoryConfigurations");

            migrationBuilder.DropColumn(
                name: "NotificationTemplates",
                table: "WorkCategoryConfigurations");

            migrationBuilder.DropColumn(
                name: "RequiredFields",
                table: "WorkCategoryConfigurations");

            migrationBuilder.DropColumn(
                name: "SLAHours",
                table: "WorkCategoryConfigurations");

            migrationBuilder.DropColumn(
                name: "ValidationRules",
                table: "WorkCategoryConfigurations");

            migrationBuilder.DropColumn(
                name: "WorkflowTemplateId",
                table: "WorkCategoryConfigurations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "BusinessVerticalId",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "ChangeReason",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "ConfigurationValue",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "DataType",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "IsEditable",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "PreviousVersionId",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "AutoAdjustmentRules",
                table: "PriorityConfigurations");

            migrationBuilder.DropColumn(
                name: "BusinessValueWeights",
                table: "PriorityConfigurations");

            migrationBuilder.DropColumn(
                name: "CapacityFactors",
                table: "PriorityConfigurations");

            migrationBuilder.DropColumn(
                name: "ColorCode",
                table: "PriorityConfigurations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PriorityConfigurations");

            migrationBuilder.DropColumn(
                name: "EscalationRules",
                table: "PriorityConfigurations");

            migrationBuilder.DropColumn(
                name: "EscalationThresholdHours",
                table: "PriorityConfigurations");

            migrationBuilder.DropColumn(
                name: "IconClass",
                table: "PriorityConfigurations");

            migrationBuilder.DropColumn(
                name: "MaxScore",
                table: "PriorityConfigurations");

            migrationBuilder.DropColumn(
                name: "MinScore",
                table: "PriorityConfigurations");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "PriorityConfigurations");

            migrationBuilder.DropColumn(
                name: "SLAHours",
                table: "PriorityConfigurations");

            migrationBuilder.DropColumn(
                name: "TimeDecayConfiguration",
                table: "PriorityConfigurations");

            migrationBuilder.DropColumn(
                name: "BusinessValueScore",
                table: "Priorities");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "Priorities");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Priorities");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Priorities");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Priorities");

            migrationBuilder.DropColumn(
                name: "ResourceImpactAssessment",
                table: "Priorities");

            migrationBuilder.DropColumn(
                name: "StrategicAlignment",
                table: "Priorities");

            migrationBuilder.DropColumn(
                name: "VotedDate",
                table: "Priorities");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Priorities");

            migrationBuilder.DropColumn(
                name: "BusinessVerticalId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "CurrentUtilization",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DepartmentCode",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "ResourceCapacity",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "SkillMatrix",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "VotingWeight",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropColumn(
                name: "ChangeReason",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropColumn(
                name: "ImplementationNotes",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropColumn(
                name: "ImplementedDate",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropColumn(
                name: "RejectedReason",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropColumn(
                name: "RequestedDate",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropColumn(
                name: "RollbackDate",
                table: "ConfigurationChangeRequests");

            migrationBuilder.DropColumn(
                name: "AccessLevel",
                table: "CapabilityDepartmentMappings");

            migrationBuilder.DropColumn(
                name: "CanApprove",
                table: "CapabilityDepartmentMappings");

            migrationBuilder.DropColumn(
                name: "CanCreate",
                table: "CapabilityDepartmentMappings");

            migrationBuilder.DropColumn(
                name: "CanModify",
                table: "CapabilityDepartmentMappings");

            migrationBuilder.DropColumn(
                name: "CanView",
                table: "CapabilityDepartmentMappings");

            migrationBuilder.DropColumn(
                name: "CapabilityId",
                table: "CapabilityDepartmentMappings");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CapabilityDepartmentMappings");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "CapabilityDepartmentMappings");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "CapabilityDepartmentMappings");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "CapabilityDepartmentMappings");

            migrationBuilder.DropColumn(
                name: "Configuration",
                table: "BusinessVerticals");

            migrationBuilder.DropColumn(
                name: "ConfigurationHistory",
                table: "BusinessVerticals");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "BusinessVerticals");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "BusinessVerticals");

            migrationBuilder.DropColumn(
                name: "StrategicImportance",
                table: "BusinessVerticals");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "BusinessVerticals");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "BusinessCapabilities");

            migrationBuilder.DropColumn(
                name: "Configuration",
                table: "BusinessCapabilities");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "BusinessCapabilities");

            migrationBuilder.DropColumn(
                name: "DependencyMap",
                table: "BusinessCapabilities");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "BusinessCapabilities");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "BusinessCapabilities");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "BusinessCapabilities");

            migrationBuilder.DropColumn(
                name: "ResourceRequirements",
                table: "BusinessCapabilities");

            migrationBuilder.DropColumn(
                name: "SubCategory",
                table: "BusinessCapabilities");

            migrationBuilder.DropColumn(
                name: "TechnicalOwner",
                table: "BusinessCapabilities");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "BusinessCapabilities");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "IPAddress",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "NewValue",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "SecurityContext",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "AllowedTransitions",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "ApprovalRequired",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "AutoTransition",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "ChangeHistory",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "NotificationTemplate",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "RequiredRoles",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "SLAHours",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "StageType",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "WorkflowStages");

            migrationBuilder.DropColumn(
                name: "AggregateId",
                table: "EventStore");

            migrationBuilder.DropColumn(
                name: "CausationId",
                table: "EventStore");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "EventStore");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "EventStore");

            migrationBuilder.DropColumn(
                name: "EventVersion",
                table: "EventStore");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "EventStore");

            migrationBuilder.DropColumn(
                name: "WorkRequestId",
                table: "EventStore");

            migrationBuilder.RenameTable(
                name: "WorkflowStages",
                newName: "WorkflowStageConfigurations");

            migrationBuilder.RenameTable(
                name: "EventStore",
                newName: "EventStores");

            migrationBuilder.RenameColumn(
                name: "SubmitterId",
                table: "WorkRequests",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "PriorityLevel",
                table: "WorkRequests",
                newName: "EstimatedHours");

            migrationBuilder.RenameIndex(
                name: "IX_WorkRequests_SubmitterId",
                table: "WorkRequests",
                newName: "IX_WorkRequests_UserId");

            migrationBuilder.RenameColumn(
                name: "ValidationRules",
                table: "WorkflowTransitions",
                newName: "Settings");

            migrationBuilder.RenameColumn(
                name: "TransitionName",
                table: "WorkflowTransitions",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ToStageId",
                table: "WorkflowTransitions",
                newName: "TargetStageId");

            migrationBuilder.RenameColumn(
                name: "FromStageId",
                table: "WorkflowTransitions",
                newName: "SourceStageId");

            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
                table: "WorkCategoryConfigurations",
                newName: "Settings");

            migrationBuilder.RenameColumn(
                name: "CategoryName",
                table: "WorkCategoryConfigurations",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
                table: "SystemConfigurations",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "ConfigurationKey",
                table: "SystemConfigurations",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "PriorityName",
                table: "PriorityConfigurations",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "NotificationSettings",
                table: "PriorityConfigurations",
                newName: "Settings");

            migrationBuilder.RenameColumn(
                name: "VotedById",
                table: "Priorities",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Vote",
                table: "Priorities",
                newName: "PriorityLevel");

            migrationBuilder.RenameIndex(
                name: "IX_Priorities_VotedById",
                table: "Priorities",
                newName: "IX_Priorities_UserId");

            migrationBuilder.RenameColumn(
                name: "RequestedValue",
                table: "ConfigurationChangeRequests",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "RequestedById",
                table: "ConfigurationChangeRequests",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ConfigurationId",
                table: "ConfigurationChangeRequests",
                newName: "BusinessVerticalId");

            migrationBuilder.RenameIndex(
                name: "IX_ConfigurationChangeRequests_RequestedById",
                table: "ConfigurationChangeRequests",
                newName: "IX_ConfigurationChangeRequests_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ConfigurationChangeRequests_ConfigurationId",
                table: "ConfigurationChangeRequests",
                newName: "IX_ConfigurationChangeRequests_BusinessVerticalId");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "CapabilityDepartmentMappings",
                newName: "BusinessCapabilityId");

            migrationBuilder.RenameColumn(
                name: "WorkRequestId",
                table: "AuditTrails",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "OldValue",
                table: "AuditTrails",
                newName: "Details");

            migrationBuilder.RenameColumn(
                name: "ChangedDate",
                table: "AuditTrails",
                newName: "ModifiedDate");

            migrationBuilder.RenameColumn(
                name: "ChangedById",
                table: "AuditTrails",
                newName: "EntityId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditTrails_WorkRequestId",
                table: "AuditTrails",
                newName: "IX_AuditTrails_UserId");

            migrationBuilder.RenameColumn(
                name: "ValidationRules",
                table: "WorkflowStageConfigurations",
                newName: "Settings");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "WorkRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "WorkRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,2)",
                oldDefaultValue: 0.0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "BusinessValue",
                table: "WorkRequests",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,2)",
                oldDefaultValue: 0.5m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "WorkRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "WorkCategory",
                table: "WorkRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "BusinessVerticalId",
                table: "WorkflowTransitions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "WorkflowTransitions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WorkCategoryConfigurations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SkillSet",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "{}");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentWorkload",
                table: "Users",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldDefaultValue: 0.0m);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SystemConfigurations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PriorityConfigurations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PriorityConfigurations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Priorities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ConfigurationChangeRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "RequestType",
                table: "ConfigurationChangeRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ConfigurationChangeRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BusinessVerticals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "BusinessCapabilities",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Metadata",
                table: "AuditTrails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "BusinessVerticalId",
                table: "AuditTrails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "AuditTrails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "EntityName",
                table: "AuditTrails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AuditTrails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WorkflowStageConfigurations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "BusinessVerticalId",
                table: "WorkflowStageConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "EventStores",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "EventStores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "EventStores",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkflowStageConfigurations",
                table: "WorkflowStageConfigurations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventStores",
                table: "EventStores",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "EventSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ActiveDirectorySid",
                table: "Users",
                column: "ActiveDirectorySid",
                unique: true,
                filter: "[ActiveDirectorySid] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_WindowsIdentity",
                table: "Users",
                column: "WindowsIdentity",
                unique: true,
                filter: "[WindowsIdentity] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PriorityConfigurations_BusinessVerticalId",
                table: "PriorityConfigurations",
                column: "BusinessVerticalId");

            migrationBuilder.CreateIndex(
                name: "IX_CapabilityDepartmentMappings_BusinessCapabilityId",
                table: "CapabilityDepartmentMappings",
                column: "BusinessCapabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_BusinessVerticalId",
                table: "AuditTrails",
                column: "BusinessVerticalId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStageConfigurations_BusinessVerticalId",
                table: "WorkflowStageConfigurations",
                column: "BusinessVerticalId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditTrails_BusinessVerticals_BusinessVerticalId",
                table: "AuditTrails",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditTrails_Users_UserId",
                table: "AuditTrails",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessCapabilities_BusinessVerticals_BusinessVerticalId",
                table: "BusinessCapabilities",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CapabilityDepartmentMappings_BusinessCapabilities_BusinessCapabilityId",
                table: "CapabilityDepartmentMappings",
                column: "BusinessCapabilityId",
                principalTable: "BusinessCapabilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationChangeRequests_BusinessVerticals_BusinessVerticalId",
                table: "ConfigurationChangeRequests",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfigurationChangeRequests_Users_UserId",
                table: "ConfigurationChangeRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Priorities_Users_UserId",
                table: "Priorities",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PriorityConfigurations_BusinessVerticals_BusinessVerticalId",
                table: "PriorityConfigurations",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_BusinessVerticals_BusinessVerticalId",
                table: "Users",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Departments_DepartmentId",
                table: "Users",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkCategoryConfigurations_BusinessVerticals_BusinessVerticalId",
                table: "WorkCategoryConfigurations",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowStageConfigurations_BusinessVerticals_BusinessVerticalId",
                table: "WorkflowStageConfigurations",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowTransitions_BusinessVerticals_BusinessVerticalId",
                table: "WorkflowTransitions",
                column: "BusinessVerticalId",
                principalTable: "BusinessVerticals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRequests_Users_UserId",
                table: "WorkRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
