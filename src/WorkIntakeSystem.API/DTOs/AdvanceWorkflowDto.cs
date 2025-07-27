using WorkIntakeSystem.Core.Enums;

namespace WorkIntakeSystem.API.DTOs
{
    public class AdvanceWorkflowDto
    {
        public WorkflowStage NextStage { get; set; }
        public string? Comments { get; set; }
    }
} 