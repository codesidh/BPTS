using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace WorkIntakeSystem.Tests.Performance
{
    /// <summary>
    /// Performance benchmarks for the Work Intake System
    /// </summary>
    public class PerformanceBenchmarks : IClassFixture<LoadTestFixture>
    {
        private readonly LoadTestFixture _fixture;
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _client;

        public PerformanceBenchmarks(LoadTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
            _client = _fixture.CreateClient();
        }

        #region API Performance Benchmarks

        [Fact]
        public async Task Benchmark_Authentication_Performance()
        {
            const int iterations = 100;
            var results = new List<BenchmarkResult>();

            for (int i = 0; i < iterations; i++)
            {
                var result = await BenchmarkAuthentication($"benchmarkuser{i}@test.com");
                results.Add(result);
            }

            var summary = AnalyzeBenchmarkResults(results, "Authentication");
            LogBenchmarkResults(summary);

            // Performance assertions
            Assert.True(summary.AverageResponseTime <= 500, $"Authentication average response time {summary.AverageResponseTime}ms exceeds 500ms");
            Assert.True(summary.Percentile95 <= 1000, $"Authentication 95th percentile {summary.Percentile95}ms exceeds 1000ms");
            Assert.True(summary.SuccessRate >= 0.95, $"Authentication success rate {summary.SuccessRate:P} is below 95%");
        }

        [Fact]
        public async Task Benchmark_WorkRequestCRUD_Performance()
        {
            const int iterations = 50;
            var results = new List<BenchmarkResult>();

            // Authenticate once
            var authResult = await AuthenticateUser("benchmarkuser@test.com");
            Assert.True(authResult.Success, "Authentication failed for benchmark");

            for (int i = 0; i < iterations; i++)
            {
                var result = await BenchmarkWorkRequestCRUD($"Benchmark Work Request {i}");
                results.Add(result);
            }

            var summary = AnalyzeBenchmarkResults(results, "Work Request CRUD");
            LogBenchmarkResults(summary);

            // Performance assertions
            Assert.True(summary.AverageResponseTime <= 200, $"Work Request CRUD average response time {summary.AverageResponseTime}ms exceeds 200ms");
            Assert.True(summary.Percentile95 <= 500, $"Work Request CRUD 95th percentile {summary.Percentile95}ms exceeds 500ms");
            Assert.True(summary.SuccessRate >= 0.98, $"Work Request CRUD success rate {summary.SuccessRate:P} is below 98%");
        }

        [Fact]
        public async Task Benchmark_PriorityCalculation_Performance()
        {
            const int iterations = 30;
            var results = new List<BenchmarkResult>();

            // Authenticate once
            var authResult = await AuthenticateUser("benchmarkuser@test.com");
            Assert.True(authResult.Success, "Authentication failed for benchmark");

            for (int i = 0; i < iterations; i++)
            {
                var result = await BenchmarkPriorityCalculation();
                results.Add(result);
            }

            var summary = AnalyzeBenchmarkResults(results, "Priority Calculation");
            LogBenchmarkResults(summary);

            // Performance assertions
            Assert.True(summary.AverageResponseTime <= 1000, $"Priority calculation average response time {summary.AverageResponseTime}ms exceeds 1000ms");
            Assert.True(summary.Percentile95 <= 2000, $"Priority calculation 95th percentile {summary.Percentile95}ms exceeds 2000ms");
            Assert.True(summary.SuccessRate >= 0.95, $"Priority calculation success rate {summary.SuccessRate:P} is below 95%");
        }

        [Fact]
        public async Task Benchmark_AnalyticsQueries_Performance()
        {
            const int iterations = 20;
            var results = new List<BenchmarkResult>();

            // Authenticate once
            var authResult = await AuthenticateUser("benchmarkuser@test.com");
            Assert.True(authResult.Success, "Authentication failed for benchmark");

            var analyticsEndpoints = new[]
            {
                "/api/analytics/executive-dashboard",
                "/api/analytics/department-dashboard",
                "/api/analytics/workload-forecast",
                "/api/analytics/priority-trends",
                "/api/analytics/completion-predictions"
            };

            for (int i = 0; i < iterations; i++)
            {
                var endpoint = analyticsEndpoints[i % analyticsEndpoints.Length];
                var result = await BenchmarkAnalyticsQuery(endpoint);
                results.Add(result);
            }

            var summary = AnalyzeBenchmarkResults(results, "Analytics Queries");
            LogBenchmarkResults(summary);

            // Performance assertions
            Assert.True(summary.AverageResponseTime <= 2000, $"Analytics queries average response time {summary.AverageResponseTime}ms exceeds 2000ms");
            Assert.True(summary.Percentile95 <= 5000, $"Analytics queries 95th percentile {summary.Percentile95}ms exceeds 5000ms");
            Assert.True(summary.SuccessRate >= 0.90, $"Analytics queries success rate {summary.SuccessRate:P} is below 90%");
        }

        [Fact]
        public async Task Benchmark_CachePerformance()
        {
            const int iterations = 100;
            var results = new List<BenchmarkResult>();

            // Authenticate once
            var authResult = await AuthenticateUser("benchmarkuser@test.com");
            Assert.True(authResult.Success, "Authentication failed for benchmark");

            var cacheEndpoints = new[]
            {
                "/api/workrequests",
                "/api/departments",
                "/api/businessverticals",
                "/api/analytics/executive-dashboard"
            };

            for (int i = 0; i < iterations; i++)
            {
                var endpoint = cacheEndpoints[i % cacheEndpoints.Length];
                var result = await BenchmarkCacheOperation(endpoint);
                results.Add(result);
            }

            var summary = AnalyzeBenchmarkResults(results, "Cache Performance");
            LogBenchmarkResults(summary);

            // Cache performance should be very fast
            Assert.True(summary.AverageResponseTime <= 50, $"Cache performance average response time {summary.AverageResponseTime}ms exceeds 50ms");
            Assert.True(summary.Percentile95 <= 100, $"Cache performance 95th percentile {summary.Percentile95}ms exceeds 100ms");
            Assert.True(summary.SuccessRate >= 0.99, $"Cache performance success rate {summary.SuccessRate:P} is below 99%");
        }

        #endregion

        #region Database Performance Benchmarks

        [Fact]
        public async Task Benchmark_DatabaseQuery_Performance()
        {
            const int iterations = 50;
            var results = new List<BenchmarkResult>();

            // Authenticate once
            var authResult = await AuthenticateUser("benchmarkuser@test.com");
            Assert.True(authResult.Success, "Authentication failed for benchmark");

            for (int i = 0; i < iterations; i++)
            {
                var result = await BenchmarkDatabaseQuery();
                results.Add(result);
            }

            var summary = AnalyzeBenchmarkResults(results, "Database Query");
            LogBenchmarkResults(summary);

            // Database performance assertions
            Assert.True(summary.AverageResponseTime <= 100, $"Database query average response time {summary.AverageResponseTime}ms exceeds 100ms");
            Assert.True(summary.Percentile95 <= 200, $"Database query 95th percentile {summary.Percentile95}ms exceeds 200ms");
            Assert.True(summary.SuccessRate >= 0.99, $"Database query success rate {summary.SuccessRate:P} is below 99%");
        }

        [Fact]
        public async Task Benchmark_DatabaseWrite_Performance()
        {
            const int iterations = 30;
            var results = new List<BenchmarkResult>();

            // Authenticate once
            var authResult = await AuthenticateUser("benchmarkuser@test.com");
            Assert.True(authResult.Success, "Authentication failed for benchmark");

            for (int i = 0; i < iterations; i++)
            {
                var result = await BenchmarkDatabaseWrite($"Benchmark Write {i}");
                results.Add(result);
            }

            var summary = AnalyzeBenchmarkResults(results, "Database Write");
            LogBenchmarkResults(summary);

            // Database write performance assertions
            Assert.True(summary.AverageResponseTime <= 200, $"Database write average response time {summary.AverageResponseTime}ms exceeds 200ms");
            Assert.True(summary.Percentile95 <= 500, $"Database write 95th percentile {summary.Percentile95}ms exceeds 500ms");
            Assert.True(summary.SuccessRate >= 0.98, $"Database write success rate {summary.SuccessRate:P} is below 98%");
        }

        #endregion

        #region Memory and Resource Benchmarks

        [Fact]
        public async Task Benchmark_MemoryUsage()
        {
            const int iterations = 100;
            var memorySnapshots = new List<MemorySnapshot>();

            for (int i = 0; i < iterations; i++)
            {
                var snapshot = await TakeMemorySnapshot($"Iteration {i}");
                memorySnapshots.Add(snapshot);
            }

            var summary = AnalyzeMemoryUsage(memorySnapshots);
            LogMemoryBenchmarkResults(summary);

            // Memory usage assertions
            Assert.True(summary.AverageMemoryUsage <= 100 * 1024 * 1024, $"Average memory usage {summary.AverageMemoryUsage / (1024 * 1024)}MB exceeds 100MB");
            Assert.True(summary.PeakMemoryUsage <= 200 * 1024 * 1024, $"Peak memory usage {summary.PeakMemoryUsage / (1024 * 1024)}MB exceeds 200MB");
            Assert.True(summary.MemoryGrowthRate <= 0.1, $"Memory growth rate {summary.MemoryGrowthRate:P} exceeds 10%");
        }

        [Fact]
        public async Task Benchmark_ConcurrentConnections()
        {
            const int concurrentConnections = 50;
            const int requestsPerConnection = 10;
            
            var tasks = new List<Task<BenchmarkResult>>();
            
            for (int i = 0; i < concurrentConnections; i++)
            {
                tasks.Add(BenchmarkConcurrentConnection($"concurrentuser{i}@test.com", requestsPerConnection));
            }

            var results = await Task.WhenAll(tasks);
            var summary = AnalyzeBenchmarkResults(results.ToList(), "Concurrent Connections");
            LogBenchmarkResults(summary);

            // Concurrent connection assertions
            Assert.True(summary.SuccessRate >= 0.90, $"Concurrent connections success rate {summary.SuccessRate:P} is below 90%");
            Assert.True(summary.AverageResponseTime <= 1000, $"Concurrent connections average response time {summary.AverageResponseTime}ms exceeds 1000ms");
        }

        #endregion

        #region Helper Methods

        private async Task<BenchmarkResult> BenchmarkAuthentication(string email)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Register user
                var registerRequest = new
                {
                    Email = email,
                    Password = "TestPassword123!",
                    FirstName = "Benchmark",
                    LastName = "User",
                    DepartmentId = 1
                };

                var registerJson = JsonSerializer.Serialize(registerRequest);
                var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");
                
                var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
                stopwatch.Stop();

                return new BenchmarkResult
                {
                    Success = registerResponse.IsSuccessStatusCode,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Authentication"
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new BenchmarkResult
                {
                    Success = false,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Authentication",
                    Error = ex.Message
                };
            }
        }

        private async Task<BenchmarkResult> BenchmarkWorkRequestCRUD(string title)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var workRequest = new
                {
                    Title = title,
                    Description = "Benchmark work request description",
                    BusinessValue = 5,
                    Urgency = 3,
                    DepartmentId = 1,
                    BusinessVerticalId = 1,
                    EstimatedHours = 40
                };

                var json = JsonSerializer.Serialize(workRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _client.PostAsync("/api/workrequests", content);
                stopwatch.Stop();

                return new BenchmarkResult
                {
                    Success = response.IsSuccessStatusCode,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Work Request CRUD"
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new BenchmarkResult
                {
                    Success = false,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Work Request CRUD",
                    Error = ex.Message
                };
            }
        }

        private async Task<BenchmarkResult> BenchmarkPriorityCalculation()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var response = await _client.GetAsync("/api/analytics/priority-predictions");
                stopwatch.Stop();

                return new BenchmarkResult
                {
                    Success = response.IsSuccessStatusCode,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Priority Calculation"
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new BenchmarkResult
                {
                    Success = false,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Priority Calculation",
                    Error = ex.Message
                };
            }
        }

        private async Task<BenchmarkResult> BenchmarkAnalyticsQuery(string endpoint)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var response = await _client.GetAsync(endpoint);
                stopwatch.Stop();

                return new BenchmarkResult
                {
                    Success = response.IsSuccessStatusCode,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Analytics Query"
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new BenchmarkResult
                {
                    Success = false,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Analytics Query",
                    Error = ex.Message
                };
            }
        }

        private async Task<BenchmarkResult> BenchmarkCacheOperation(string endpoint)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var response = await _client.GetAsync(endpoint);
                stopwatch.Stop();

                return new BenchmarkResult
                {
                    Success = response.IsSuccessStatusCode,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Cache Operation"
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new BenchmarkResult
                {
                    Success = false,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Cache Operation",
                    Error = ex.Message
                };
            }
        }

        private async Task<BenchmarkResult> BenchmarkDatabaseQuery()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var response = await _client.GetAsync("/api/workrequests");
                stopwatch.Stop();

                return new BenchmarkResult
                {
                    Success = response.IsSuccessStatusCode,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Database Query"
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new BenchmarkResult
                {
                    Success = false,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Database Query",
                    Error = ex.Message
                };
            }
        }

        private async Task<BenchmarkResult> BenchmarkDatabaseWrite(string title)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var workRequest = new
                {
                    Title = title,
                    Description = "Benchmark database write",
                    BusinessValue = 5,
                    Urgency = 3,
                    DepartmentId = 1,
                    BusinessVerticalId = 1,
                    EstimatedHours = 40
                };

                var json = JsonSerializer.Serialize(workRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _client.PostAsync("/api/workrequests", content);
                stopwatch.Stop();

                return new BenchmarkResult
                {
                    Success = response.IsSuccessStatusCode,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Database Write"
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new BenchmarkResult
                {
                    Success = false,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Database Write",
                    Error = ex.Message
                };
            }
        }

        private async Task<BenchmarkResult> BenchmarkConcurrentConnection(string email, int requestCount)
        {
            var stopwatch = Stopwatch.StartNew();
            var successCount = 0;
            
            try
            {
                for (int i = 0; i < requestCount; i++)
                {
                    var response = await _client.GetAsync("/api/workrequests");
                    if (response.IsSuccessStatusCode)
                        successCount++;
                }
                
                stopwatch.Stop();

                return new BenchmarkResult
                {
                    Success = successCount == requestCount,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Concurrent Connection"
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new BenchmarkResult
                {
                    Success = false,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Operation = "Concurrent Connection",
                    Error = ex.Message
                };
            }
        }

        private async Task<MemorySnapshot> TakeMemorySnapshot(string iteration)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var memoryBefore = GC.GetTotalMemory(false);
            
            // Perform some operations
            var response = await _client.GetAsync("/api/workrequests");
            
            var memoryAfter = GC.GetTotalMemory(false);
            
            return new MemorySnapshot
            {
                Iteration = iteration,
                MemoryBefore = memoryBefore,
                MemoryAfter = memoryAfter,
                MemoryUsed = memoryAfter - memoryBefore,
                Timestamp = DateTime.UtcNow
            };
        }

        private async Task<RequestResult> AuthenticateUser(string email)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var loginRequest = new
                {
                    Email = email,
                    Password = "TestPassword123!"
                };

                var json = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _client.PostAsync("/api/auth/login", content);
                stopwatch.Stop();

                return new RequestResult
                {
                    Success = response.IsSuccessStatusCode,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new RequestResult
                {
                    Success = false,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    StatusCode = 0,
                    Error = ex.Message
                };
            }
        }

        private BenchmarkSummary AnalyzeBenchmarkResults(List<BenchmarkResult> results, string operation)
        {
            var successfulResults = results.Where(r => r.Success).ToList();
            var responseTimes = successfulResults.Select(r => r.ResponseTime).OrderBy(t => t).ToList();

            return new BenchmarkSummary
            {
                Operation = operation,
                TotalRequests = results.Count,
                SuccessfulRequests = successfulResults.Count,
                FailedRequests = results.Count - successfulResults.Count,
                SuccessRate = (double)successfulResults.Count / results.Count,
                AverageResponseTime = successfulResults.Any() ? successfulResults.Average(r => r.ResponseTime) : 0,
                MinResponseTime = responseTimes.Any() ? responseTimes.First() : 0,
                MaxResponseTime = responseTimes.Any() ? responseTimes.Last() : 0,
                Percentile95 = responseTimes.Any() ? responseTimes[(int)(responseTimes.Count * 0.95)] : 0,
                Percentile99 = responseTimes.Any() ? responseTimes[(int)(responseTimes.Count * 0.99)] : 0,
                Throughput = results.Count > 0 ? results.Count / (results.Max(r => r.ResponseTime) / 1000.0) : 0
            };
        }

        private MemoryBenchmarkSummary AnalyzeMemoryUsage(List<MemorySnapshot> snapshots)
        {
            var memoryUsages = snapshots.Select(s => s.MemoryUsed).ToList();
            var totalMemory = snapshots.Select(s => s.MemoryAfter).ToList();

            return new MemoryBenchmarkSummary
            {
                TotalSnapshots = snapshots.Count,
                AverageMemoryUsage = totalMemory.Average(),
                PeakMemoryUsage = totalMemory.Max(),
                MinMemoryUsage = totalMemory.Min(),
                AverageMemoryUsed = memoryUsages.Average(),
                PeakMemoryUsed = memoryUsages.Max(),
                MemoryGrowthRate = snapshots.Count > 1 ? (totalMemory.Last() - totalMemory.First()) / (double)totalMemory.First() : 0
            };
        }

        private void LogBenchmarkResults(BenchmarkSummary summary)
        {
            _output.WriteLine($"=== {summary.Operation} Benchmark Results ===");
            _output.WriteLine($"Total Requests: {summary.TotalRequests}");
            _output.WriteLine($"Successful Requests: {summary.SuccessfulRequests}");
            _output.WriteLine($"Failed Requests: {summary.FailedRequests}");
            _output.WriteLine($"Success Rate: {summary.SuccessRate:P}");
            _output.WriteLine($"Average Response Time: {summary.AverageResponseTime:F2}ms");
            _output.WriteLine($"Min Response Time: {summary.MinResponseTime}ms");
            _output.WriteLine($"Max Response Time: {summary.MaxResponseTime}ms");
            _output.WriteLine($"95th Percentile: {summary.Percentile95}ms");
            _output.WriteLine($"99th Percentile: {summary.Percentile99}ms");
            _output.WriteLine($"Throughput: {summary.Throughput:F2} req/sec");
            _output.WriteLine("==========================================");
        }

        private void LogMemoryBenchmarkResults(MemoryBenchmarkSummary summary)
        {
            _output.WriteLine($"=== Memory Usage Benchmark Results ===");
            _output.WriteLine($"Total Snapshots: {summary.TotalSnapshots}");
            _output.WriteLine($"Average Memory Usage: {summary.AverageMemoryUsage / (1024 * 1024):F2}MB");
            _output.WriteLine($"Peak Memory Usage: {summary.PeakMemoryUsage / (1024 * 1024):F2}MB");
            _output.WriteLine($"Min Memory Usage: {summary.MinMemoryUsage / (1024 * 1024):F2}MB");
            _output.WriteLine($"Average Memory Used: {summary.AverageMemoryUsed / (1024 * 1024):F2}MB");
            _output.WriteLine($"Peak Memory Used: {summary.PeakMemoryUsed / (1024 * 1024):F2}MB");
            _output.WriteLine($"Memory Growth Rate: {summary.MemoryGrowthRate:P}");
            _output.WriteLine("=======================================");
        }

        #endregion
    }

    #region Data Models

    public class BenchmarkResult
    {
        public bool Success { get; set; }
        public long ResponseTime { get; set; }
        public string Operation { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    public class BenchmarkSummary
    {
        public string Operation { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public double AverageResponseTime { get; set; }
        public long MinResponseTime { get; set; }
        public long MaxResponseTime { get; set; }
        public long Percentile95 { get; set; }
        public long Percentile99 { get; set; }
        public double Throughput { get; set; }
    }

    public class MemorySnapshot
    {
        public string Iteration { get; set; } = string.Empty;
        public long MemoryBefore { get; set; }
        public long MemoryAfter { get; set; }
        public long MemoryUsed { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class MemoryBenchmarkSummary
    {
        public int TotalSnapshots { get; set; }
        public double AverageMemoryUsage { get; set; }
        public long PeakMemoryUsage { get; set; }
        public long MinMemoryUsage { get; set; }
        public double AverageMemoryUsed { get; set; }
        public long PeakMemoryUsed { get; set; }
        public double MemoryGrowthRate { get; set; }
    }

    public class RequestResult
    {
        public bool Success { get; set; }
        public long ResponseTime { get; set; }
        public int StatusCode { get; set; }
        public string Error { get; set; } = string.Empty;
    }

    #endregion
}
