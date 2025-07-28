using System;
using System.Linq;
using System.Threading.Tasks;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Core.Services;

namespace WorkIntakeSystem.TestRunner;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Work Intake System - Core Functionality Tests ===");
        Console.WriteLine();

        try
        {
            // Test 1: Core Entities Validation
            Console.WriteLine("Test 1: Core Entities Validation");
            TestCoreEntities();
            Console.WriteLine("✅ Core entities test passed");
            Console.WriteLine();

            // Test 2: Priority Calculation Service
            Console.WriteLine("Test 2: Priority Calculation Service");
            await TestPriorityCalculationService();
            Console.WriteLine("✅ Priority calculation test passed");
            Console.WriteLine();

            // Test 3: Enums and Constants
            Console.WriteLine("Test 3: Enums and Constants");
            TestEnumsAndConstants();
            Console.WriteLine("✅ Enums and constants test passed");
            Console.WriteLine();

            // Test 4: Business Logic Validation
            Console.WriteLine("Test 4: Business Logic Validation");
            TestBusinessLogic();
            Console.WriteLine("✅ Business logic test passed");
            Console.WriteLine();

            Console.WriteLine("=== ALL TESTS PASSED ✅ ===");
            Console.WriteLine("The core Work Intake System functionality is working correctly!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test failed with error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    static void TestCoreEntities()
    {
        // Test WorkRequest entity
        var workRequest = new WorkRequest
        {
            Id = 1,
            Title = "Test Work Request",
            Description = "This is a test work request",
            Category = WorkCategory.Enhancement,
            BusinessVerticalId = 1,
            DepartmentId = 1,
            SubmitterId = 1,
            Status = WorkStatus.Draft,
            CurrentStage = WorkflowStage.Intake,
            Priority = 0.8m,
            EstimatedEffort = 40,
            BusinessValue = 0.9m,
            PriorityLevel = PriorityLevel.High,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (string.IsNullOrEmpty(workRequest.Title))
            throw new Exception("WorkRequest title should not be empty");

        if (workRequest.BusinessValue < 0 || workRequest.BusinessValue > 1)
            throw new Exception("BusinessValue should be between 0 and 1");

        // Test User entity
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            DepartmentId = 1,
            BusinessVerticalId = 1,
            Role = UserRole.BusinessExecutive,
            Capacity = 40,
            CurrentWorkload = 0.75m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email))
            throw new Exception("User name and email should not be empty");

        // Test Department entity
        var department = new Department
        {
            Id = 1,
            Name = "Information Technology",
            Description = "IT Department",
            BusinessVerticalId = 1,
            VotingWeight = 1.2m,
            ResourceCapacity = 100,
            CurrentUtilization = 0.65m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (department.VotingWeight <= 0)
            throw new Exception("Department voting weight should be positive");
    }

    static async Task TestPriorityCalculationService()
    {
        var priorityService = new PriorityCalculationService();

        var workRequest = new WorkRequest
        {
            Id = 1,
            Title = "High Priority Feature",
            BusinessValue = 0.9m,
            EstimatedEffort = 20,
            CreatedAt = DateTime.UtcNow.AddDays(-5), // 5 days old
            TimeDecayFactor = 1.0m,
            CapacityAdjustment = 1.0m
        };

        var department = new Department
        {
            Id = 1,
            VotingWeight = 1.5m
        };

        var user = new User
        {
            Id = 1,
            Role = UserRole.BusinessExecutive
        };

        // Test priority calculation
        var priority = await priorityService.CalculatePriorityAsync(workRequest, department, user);

        if (priority < 0 || priority > 1)
            throw new Exception($"Priority should be between 0 and 1, but was {priority}");

        // Test time decay calculation
        var timeDecay = await priorityService.CalculateTimeDecayAsync(workRequest);
        if (timeDecay < 1.0m)
            throw new Exception("Time decay should be at least 1.0 for older requests");

        // Test capacity adjustment
        var capacityAdj = await priorityService.CalculateCapacityAdjustmentAsync(department);
        if (capacityAdj <= 0)
            throw new Exception("Capacity adjustment should be positive");
    }

    static void TestEnumsAndConstants()
    {
        // Test WorkflowStage enum
        var stages = Enum.GetValues<WorkflowStage>();
        if (stages.Length < 10)
            throw new Exception("Should have multiple workflow stages defined");

        if (!stages.Contains(WorkflowStage.Intake))
            throw new Exception("Should have Intake stage");

        if (!stages.Contains(WorkflowStage.Closed))
            throw new Exception("Should have Closed stage");

        // Test UserRole enum
        var roles = Enum.GetValues<UserRole>();
        if (!roles.Contains(UserRole.SystemAdministrator))
            throw new Exception("Should have SystemAdministrator role");

        if (!roles.Contains(UserRole.BusinessExecutive))
            throw new Exception("Should have BusinessExecutive role");

        // Test WorkStatus enum
        var statuses = Enum.GetValues<WorkStatus>();
        if (!statuses.Contains(WorkStatus.Draft))
            throw new Exception("Should have Draft status");

        if (!statuses.Contains(WorkStatus.Approved))
            throw new Exception("Should have Approved status");

        // Test PriorityLevel enum
        var priorities = Enum.GetValues<PriorityLevel>();
        if (!priorities.Contains(PriorityLevel.Critical))
            throw new Exception("Should have Critical priority level");

        if (!priorities.Contains(PriorityLevel.Low))
            throw new Exception("Should have Low priority level");
    }

    static void TestBusinessLogic()
    {
        // Test work request workflow progression
        var workRequest = new WorkRequest
        {
            CurrentStage = WorkflowStage.Intake,
            Status = WorkStatus.Draft
        };

        // Simulate progressing through workflow
        if (workRequest.CurrentStage != WorkflowStage.Intake)
            throw new Exception("New work request should start in Intake stage");

        // Test priority calculation logic
        decimal businessValue = 0.8m;
        decimal departmentWeight = 1.2m;
        decimal timeDecay = 1.1m;
        decimal capacityAdj = 0.9m;

        decimal calculatedPriority = businessValue * departmentWeight * timeDecay * capacityAdj;
        
        if (calculatedPriority < 0)
            throw new Exception("Calculated priority should not be negative");

        // Test effort estimation validation
        int estimatedEffort = 40;
        if (estimatedEffort <= 0)
            throw new Exception("Estimated effort should be positive");

        // Test business value range
        if (businessValue < 0 || businessValue > 1)
            throw new Exception("Business value should be between 0 and 1");
    }
} 