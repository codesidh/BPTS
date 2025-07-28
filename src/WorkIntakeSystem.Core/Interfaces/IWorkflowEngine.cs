using System.Threading.Tasks;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;

namespace WorkIntakeSystem.Core.Interfaces
{
    public interface IWorkflowEngine
    {
        Task<bool> CanAdvanceAsync(WorkRequest workRequest, WorkflowStage nextStage, int userId);
        Task AdvanceAsync(WorkRequest workRequest, WorkflowStage nextStage, int userId, string? comments = null);
    }
} 