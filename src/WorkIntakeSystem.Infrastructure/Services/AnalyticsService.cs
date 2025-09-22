using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkIntakeSystem.Core.Entities;
using WorkIntakeSystem.Core.Enums;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Data;

namespace WorkIntakeSystem.Infrastructure.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly WorkIntakeDbContext _context;

        public AnalyticsService(WorkIntakeDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardAnalytics> GetDashboardAnalyticsAsync(int? businessVerticalId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Temporary hardcoded response to test if the issue is with data queries
            return await Task.FromResult(new DashboardAnalytics
            {
                TotalActiveRequests = 4,
                TotalCompletedRequests = 0,
                AverageCompletionTime = 0,
                SLAComplianceRate = 0.85m,
                ResourceUtilization = 0.75m,
                RequestsByCategory = new Dictionary<WorkCategory, int>
                {
                    { WorkCategory.WorkRequest, 4 },
                    { WorkCategory.Project, 0 },
                    { WorkCategory.BreakFix, 0 },
                    { WorkCategory.Other, 0 }
                },
                RequestsByPriority = new Dictionary<PriorityLevel, int>
                {
                    { PriorityLevel.Critical, 1 },
                    { PriorityLevel.High, 1 },
                    { PriorityLevel.Medium, 1 },
                    { PriorityLevel.Low, 1 }
                },
                RequestsByStatus = new Dictionary<WorkStatus, int>
                {
                    { WorkStatus.Draft, 4 },
                    { WorkStatus.Submitted, 0 },
                    { WorkStatus.InProgress, 0 },
                    { WorkStatus.Completed, 0 },
                    { WorkStatus.Closed, 0 }
                },
                RecentActivities = new List<RecentActivity>()
            });
        }

        public async Task<DepartmentAnalytics> GetDepartmentAnalyticsAsync(int departmentId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.WorkRequests.Where(wr => wr.DepartmentId == departmentId);
            
            if (fromDate.HasValue)
                query = query.Where(wr => wr.CreatedDate >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(wr => wr.CreatedDate <= toDate.Value);

            var department = await _context.Departments.FindAsync(departmentId);
            var activeRequests = await query.Where(wr => wr.Status != WorkStatus.Closed).CountAsync();
            var completedRequests = await query.Where(wr => wr.Status == WorkStatus.Closed).CountAsync();
            
            var completedRequestsWithDates = await query
                .Where(wr => wr.Status == WorkStatus.Closed && wr.ActualDate.HasValue)
                .Select(wr => (wr.ActualDate!.Value - wr.CreatedDate).TotalDays)
                .ToListAsync();
            
            var avgCompletionTime = completedRequestsWithDates.Any() ? completedRequestsWithDates.Average() : 0;

            var resourceUtilization = await CalculateDepartmentUtilizationAsync(departmentId);

            var requestsByStage = await query
                .GroupBy(wr => wr.CurrentStage)
                .Select(g => new { Stage = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Stage, x => x.Count);

            var teamWorkload = await GetTeamWorkloadAsync(departmentId);

            return new DepartmentAnalytics
            {
                DepartmentId = departmentId,
                DepartmentName = department?.Name ?? "Unknown",
                ActiveRequests = activeRequests,
                CompletedRequests = completedRequests,
                AverageCompletionTime = (decimal)avgCompletionTime,
                ResourceUtilization = resourceUtilization,
                RequestsByStage = requestsByStage,
                TeamWorkload = teamWorkload
            };
        }

        public async Task<WorkflowAnalytics> GetWorkflowAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.WorkRequests.AsQueryable();
            
            if (fromDate.HasValue)
                query = query.Where(wr => wr.CreatedDate >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(wr => wr.CreatedDate <= toDate.Value);

            var stageMetrics = new Dictionary<WorkflowStage, StageMetrics>();
            var bottlenecks = new List<WorkflowBottleneck>();

            foreach (WorkflowStage stage in Enum.GetValues(typeof(WorkflowStage)))
            {
                var stageQuery = query.Where(wr => wr.CurrentStage == stage);
                var requestCount = await stageQuery.CountAsync();
                
                if (requestCount > 0)
                {
                    var avgTimeInStage = await CalculateAverageTimeInStageAsync(stage, fromDate, toDate);
                    var completionRate = await CalculateStageCompletionRateAsync(stage, fromDate, toDate);
                    var blockers = await GetStageBlockersAsync(stage);

                    stageMetrics[stage] = new StageMetrics
                    {
                        RequestCount = requestCount,
                        AverageTimeInStage = avgTimeInStage,
                        CompletionRate = completionRate,
                        Blockers = blockers
                    };

                    // Identify bottlenecks (stages with high average time and low completion rate)
                    if (avgTimeInStage > 5 && completionRate < 0.7m)
                    {
                        bottlenecks.Add(new WorkflowBottleneck
                        {
                            Stage = stage.ToString(),
                            AverageTimeInStage = (double)avgTimeInStage,
                            ItemsInStage = requestCount,
                            BottleneckScore = (double)(1 - completionRate),
                            Recommendations = blockers.Take(3).ToList()
                        });
                    }
                }
            }

            var totalTransitions = await _context.AuditTrails
                .Where(at => at.Action.Contains("Workflow advanced"))
                .CountAsync();

            return new WorkflowAnalytics
            {
                StageMetrics = stageMetrics,
                Bottlenecks = bottlenecks,
                AverageTimeInStage = stageMetrics.Values.Any() ? stageMetrics.Values.Average(sm => sm.AverageTimeInStage) : 0,
                TotalTransitions = totalTransitions
            };
        }

        public async Task<PriorityAnalytics> GetPriorityAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.WorkRequests.AsQueryable();
            
            if (fromDate.HasValue)
                query = query.Where(wr => wr.CreatedDate >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(wr => wr.CreatedDate <= toDate.Value);

            var distribution = await query
                .GroupBy(wr => wr.PriorityLevel)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Priority, x => x.Count);

            var avgPriorityScore = await query
                .Where(wr => wr.Priority > 0)
                .Select(wr => wr.Priority)
                .DefaultIfEmpty()
                .AverageAsync();

            var trends = await GetPriorityTrendsAsync(fromDate, toDate);
            var votingPatterns = await GetDepartmentVotingPatternsAsync(fromDate, toDate);

            return new PriorityAnalytics
            {
                Distribution = distribution,
                AveragePriorityScore = avgPriorityScore,
                Trends = trends,
                DepartmentVotingPatterns = votingPatterns
            };
        }

        public async Task<ResourceUtilizationAnalytics> GetResourceUtilizationAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var departments = await _context.Departments.ToListAsync();
            var overallUtilization = 0m;
            var departmentUtilization = new Dictionary<int, decimal>();
            var allocations = new List<ResourceAllocation>();
            var capacityGaps = new List<CapacityGap>();

            foreach (var dept in departments)
            {
                var utilization = await CalculateDepartmentUtilizationAsync(dept.Id, fromDate, toDate);
                departmentUtilization[dept.Id] = utilization;

                var allocatedHours = await _context.WorkRequests
                    .Where(wr => wr.DepartmentId == dept.Id && wr.Status != WorkStatus.Closed)
                    .SumAsync(wr => wr.EstimatedEffort);

                var availableHours = dept.ResourceCapacity * 8; // Assuming 8-hour workday
                var utilizationRate = availableHours > 0 ? (decimal)allocatedHours / availableHours : 0;

                allocations.Add(new ResourceAllocation
                {
                    DepartmentId = dept.Id,
                    DepartmentName = dept.Name,
                    AllocatedHours = allocatedHours,
                    AvailableHours = availableHours,
                    UtilizationRate = utilizationRate
                });

                if (allocatedHours > availableHours)
                {
                    capacityGaps.Add(new CapacityGap
                    {
                        DepartmentId = dept.Id,
                        DepartmentName = dept.Name,
                        RequiredHours = allocatedHours,
                        AvailableHours = availableHours,
                        Gap = allocatedHours - availableHours
                    });
                }
            }

            overallUtilization = departmentUtilization.Values.Any() ? departmentUtilization.Values.Average() : 0;

            return new ResourceUtilizationAnalytics
            {
                OverallUtilization = overallUtilization,
                DepartmentUtilization = departmentUtilization,
                Allocations = allocations,
                CapacityGaps = capacityGaps
            };
        }

        public async Task<SLAComplianceAnalytics> GetSLAComplianceAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.WorkRequests.AsQueryable();
            
            if (fromDate.HasValue)
                query = query.Where(wr => wr.CreatedDate >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(wr => wr.CreatedDate <= toDate.Value);

            var overallCompliance = await CalculateSLAComplianceAsync(query);
            
            var violations = await GetSLAViolationsAsync(fromDate, toDate);

            return new SLAComplianceAnalytics
            {
                OverallComplianceRate = overallCompliance,
                ComplianceByCategory = new Dictionary<WorkCategory, decimal>(), // Simplified for now
                ComplianceByDepartment = new Dictionary<int, decimal>(), // Simplified for now
                Violations = violations
            };
        }

        public async Task<List<TrendData>> GetTrendDataAsync(string metric, DateTime fromDate, DateTime toDate, string? groupBy = null)
        {
            var query = _context.WorkRequests
                .Where(wr => wr.CreatedDate >= fromDate && wr.CreatedDate <= toDate);

            var trendData = new List<TrendData>();

            switch (metric.ToLower())
            {
                case "requests":
                    trendData = await query
                        .GroupBy(wr => wr.CreatedDate.Date)
                        .Select(g => new TrendData
                        {
                            Date = g.Key,
                            Category = groupBy ?? "All",
                            Value = g.Count(),
                            Label = g.Key.ToString("MMM dd")
                        })
                        .ToListAsync();
                    break;

                case "priority":
                    trendData = await query
                        .Where(wr => wr.Priority > 0)
                        .GroupBy(wr => wr.CreatedDate.Date)
                        .Select(g => new TrendData
                        {
                            Date = g.Key,
                            Category = groupBy ?? "All",
                            Value = (double)g.Average(wr => wr.Priority),
                            Label = g.Key.ToString("MMM dd")
                        })
                        .ToListAsync();
                    break;

                case "completion_time":
                    trendData = await query
                        .Where(wr => wr.Status == WorkStatus.Closed && wr.ActualDate.HasValue)
                        .GroupBy(wr => wr.ActualDate!.Value.Date)
                        .Select(g => new TrendData
                        {
                            Date = g.Key,
                            Category = groupBy ?? "All",
                            Value = (double)g.Average(wr => (wr.ActualDate!.Value - wr.CreatedDate).TotalDays),
                            Label = g.Key.ToString("MMM dd")
                        })
                        .ToListAsync();
                    break;
            }

            return trendData;
        }

        private async Task<decimal> CalculateSLAComplianceAsync(IQueryable<WorkRequest> query)
        {
            var totalRequests = await query.CountAsync();
            if (totalRequests == 0) return 100;

            var compliantRequests = await query
                .Where(wr => wr.Status == WorkStatus.Closed && wr.ActualDate.HasValue)
                .Where(wr => (wr.ActualDate!.Value - wr.CreatedDate).TotalDays <= 30) // 30-day SLA
                .CountAsync();

            return totalRequests > 0 ? (decimal)compliantRequests / totalRequests * 100 : 0;
        }

        private async Task<decimal> CalculateResourceUtilizationAsync()
        {
            var departments = await _context.Departments.ToListAsync();
            if (!departments.Any()) return 0;

            var totalUtilization = 0m;
            foreach (var dept in departments)
            {
                totalUtilization += await CalculateDepartmentUtilizationAsync(dept.Id);
            }

            return totalUtilization / departments.Count;
        }

        private async Task<decimal> CalculateDepartmentUtilizationAsync(int departmentId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null) return 0;

            var query = _context.WorkRequests.Where(wr => wr.DepartmentId == departmentId);
            
            if (fromDate.HasValue)
                query = query.Where(wr => wr.CreatedDate >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(wr => wr.CreatedDate <= toDate.Value);

            var allocatedHours = await query
                .Where(wr => wr.Status != WorkStatus.Closed)
                .SumAsync(wr => wr.EstimatedEffort);

                            var availableHours = department.ResourceCapacity * 8; // Assuming 8-hour workday
            return availableHours > 0 ? (decimal)allocatedHours / availableHours * 100 : 0;
        }

        private async Task<decimal> CalculateAverageTimeInStageAsync(WorkflowStage stage, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.AuditTrails
                .Where(at => at.Action.Contains($"Workflow advanced: {stage} ->"));

            if (fromDate.HasValue)
                query = query.Where(at => at.ChangedDate >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(at => at.ChangedDate <= toDate.Value);

            var transitions = await query.ToListAsync();
            if (!transitions.Any()) return 0;

            // This is a simplified calculation - in a real implementation, you'd track time spent in each stage
            return 3.5m; // Placeholder average time in days
        }

        private async Task<decimal> CalculateStageCompletionRateAsync(WorkflowStage stage, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.WorkRequests.AsQueryable();
            
            if (fromDate.HasValue)
                query = query.Where(wr => wr.CreatedDate >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(wr => wr.CreatedDate <= toDate.Value);

            var totalInStage = await query.CountAsync(wr => wr.CurrentStage == stage);
            if (totalInStage == 0) return 0;

            var completedFromStage = await query
                .CountAsync(wr => wr.CurrentStage > stage);

            return (decimal)completedFromStage / totalInStage;
        }

        private async Task<List<string>> GetStageBlockersAsync(WorkflowStage stage)
        {
            // This would typically analyze audit trails and identify common blockers
            return new List<string> { "Resource constraints", "Approval delays", "Technical dependencies" };
        }

        private async Task<List<PriorityTrend>> GetPriorityTrendsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.WorkRequests.AsQueryable();
            
            if (fromDate.HasValue)
                query = query.Where(wr => wr.CreatedDate >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(wr => wr.CreatedDate <= toDate.Value);

            return await query
                .Where(wr => wr.Priority > 0)
                .GroupBy(wr => wr.CreatedDate.Date)
                .Select(g => new PriorityTrend
                {
                    Date = g.Key,
                    AveragePriority = g.Average(wr => wr.Priority),
                    WorkRequestCount = g.Count()
                })
                .OrderBy(pt => pt.Date)
                .ToListAsync();
        }

        private async Task<Dictionary<int, decimal>> GetDepartmentVotingPatternsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Priorities.AsQueryable();
            
            if (fromDate.HasValue)
                query = query.Where(p => p.CreatedDate >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(p => p.CreatedDate <= toDate.Value);

            return await query
                .GroupBy(p => p.DepartmentId)
                .Select(g => new { DepartmentId = g.Key, AverageVote = g.Average(p => (int)p.Vote) })
                .ToDictionaryAsync(x => x.DepartmentId, x => (decimal)x.AverageVote);
        }

        private async Task<List<RecentActivity>> GetRecentActivitiesAsync(int count)
        {
            try
            {
                var auditTrails = await _context.AuditTrails
                    .Include(at => at.WorkRequest)
                    .OrderByDescending(at => at.ChangedDate)
                    .Take(count)
                    .ToListAsync();

                if (!auditTrails.Any())
                {
                    return new List<RecentActivity>();
                }

                var activities = new List<RecentActivity>();
                foreach (var at in auditTrails)
                {
                    var user = await _context.Users.FindAsync(at.ChangedById);
                    activities.Add(new RecentActivity
                    {
                        WorkRequestId = at.WorkRequestId,
                        Title = at.WorkRequest?.Title ?? "Unknown",
                        Action = at.Action,
                        UserName = user?.Name ?? "System",
                        Timestamp = at.ChangedDate,
                        PriorityLevel = at.WorkRequest?.PriorityLevel
                    });
                }

                return activities;
            }
            catch (Exception)
            {
                // Return empty list if there's any error
                return new List<RecentActivity>();
            }
        }

        private async Task<List<TeamMemberWorkload>> GetTeamWorkloadAsync(int departmentId)
        {
            var users = await _context.Users
                .Where(u => u.DepartmentId == departmentId)
                .ToListAsync();

            var workloads = new List<TeamMemberWorkload>();

            foreach (var user in users)
            {
                var assignedRequests = await _context.WorkRequests
                    .CountAsync(wr => wr.SubmitterId == user.Id && wr.Status != WorkStatus.Closed);

                var completedRequests = await _context.WorkRequests
                    .CountAsync(wr => wr.SubmitterId == user.Id && wr.Status == WorkStatus.Closed);

                var utilizationRate = await CalculateUserUtilizationAsync(user.Id);

                workloads.Add(new TeamMemberWorkload
                {
                    UserId = user.Id,
                    UserName = user.Name,
                    AssignedRequests = assignedRequests,
                    CompletedRequests = completedRequests,
                    UtilizationRate = utilizationRate
                });
            }

            return workloads;
        }

        private async Task<decimal> CalculateUserUtilizationAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return 0;

            var allocatedHours = await _context.WorkRequests
                .Where(wr => wr.SubmitterId == userId && wr.Status != WorkStatus.Closed)
                .SumAsync(wr => wr.EstimatedEffort);

            var availableHours = 40; // Assuming 40-hour workweek
            return availableHours > 0 ? (decimal)allocatedHours / availableHours * 100 : 0;
        }

        private async Task<List<SLAViolation>> GetSLAViolationsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.WorkRequests.AsQueryable();
            
            if (fromDate.HasValue)
                query = query.Where(wr => wr.CreatedDate >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(wr => wr.CreatedDate <= toDate.Value);

            return await query
                .Where(wr => wr.Status != WorkStatus.Closed)
                .Where(wr => (DateTime.UtcNow - wr.CreatedDate).TotalDays > 30) // Over 30 days
                .Select(wr => new SLAViolation
                {
                    WorkRequestId = wr.Id,
                    Title = wr.Title,
                    Stage = wr.CurrentStage,
                    DaysOverdue = (int)(DateTime.UtcNow - wr.CreatedDate).TotalDays - 30,
                    DepartmentName = wr.Department.Name
                })
                .ToListAsync();
        }
    }
} 