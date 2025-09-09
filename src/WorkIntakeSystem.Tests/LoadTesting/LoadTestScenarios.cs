using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace WorkIntakeSystem.Tests.LoadTesting
{
    /// <summary>
    /// Comprehensive load testing scenarios for the Work Intake System
    /// </summary>
    public class LoadTestScenarios : IClassFixture<LoadTestFixture>
    {
        private readonly LoadTestFixture _fixture;
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _client;

        public LoadTestScenarios(LoadTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
            _client = _fixture.CreateClient();
        }

        #region API Endpoint Load Tests

        [Fact]
        public async Task LoadTest_Authentication_ConcurrentUsers()
        {
            // Test concurrent user authentication
            const int concurrentUsers = 50;
            const int requestsPerUser = 10;
            
            var tasks = new List<Task<LoadTestResult>>();
            
            for (int i = 0; i < concurrentUsers; i++)
            {
                tasks.Add(TestConcurrentAuthentication($"user{i}@test.com", requestsPerUser));
            }

            var results = await Task.WhenAll(tasks);
            var summary = AnalyzeResults(results, "Authentication Load Test");

            _output.WriteLine($"Authentication Load Test Results:");
            _output.WriteLine($"- Total Requests: {summary.TotalRequests}");
            _output.WriteLine($"- Successful Requests: {summary.SuccessfulRequests}");
            _output.WriteLine($"- Failed Requests: {summary.FailedRequests}");
            _output.WriteLine($"- Average Response Time: {summary.AverageResponseTime}ms");
            _output.WriteLine($"- 95th Percentile: {summary.Percentile95}ms");
            _output.WriteLine($"- Throughput: {summary.Throughput} req/sec");

            // Assertions
            Assert.True(summary.SuccessRate >= 0.95, $"Success rate {summary.SuccessRate:P} is below 95%");
            Assert.True(summary.AverageResponseTime <= 1000, $"Average response time {summary.AverageResponseTime}ms exceeds 1000ms");
            Assert.True(summary.Percentile95 <= 2000, $"95th percentile {summary.Percentile95}ms exceeds 2000ms");
        }

        [Fact]
        public async Task LoadTest_WorkRequestCRUD_ConcurrentOperations()
        {
            // Test concurrent work request operations
            const int concurrentUsers = 30;
            const int operationsPerUser = 20;
            
            var tasks = new List<Task<LoadTestResult>>();
            
            for (int i = 0; i < concurrentUsers; i++)
            {
                tasks.Add(TestConcurrentWorkRequestOperations($"user{i}@test.com", operationsPerUser));
            }

            var results = await Task.WhenAll(tasks);
            var summary = AnalyzeResults(results, "Work Request CRUD Load Test");

            _output.WriteLine($"Work Request CRUD Load Test Results:");
            _output.WriteLine($"- Total Operations: {summary.TotalRequests}");
            _output.WriteLine($"- Successful Operations: {summary.SuccessfulRequests}");
            _output.WriteLine($"- Failed Operations: {summary.FailedRequests}");
            _output.WriteLine($"- Average Response Time: {summary.AverageResponseTime}ms");
            _output.WriteLine($"- 95th Percentile: {summary.Percentile95}ms");
            _output.WriteLine($"- Throughput: {summary.Throughput} ops/sec");

            // Assertions
            Assert.True(summary.SuccessRate >= 0.90, $"Success rate {summary.SuccessRate:P} is below 90%");
            Assert.True(summary.AverageResponseTime <= 500, $"Average response time {summary.AverageResponseTime}ms exceeds 500ms");
        }

        [Fact]
        public async Task LoadTest_PriorityCalculation_ConcurrentCalculations()
        {
            // Test concurrent priority calculations
            const int concurrentUsers = 25;
            const int calculationsPerUser = 15;
            
            var tasks = new List<Task<LoadTestResult>>();
            
            for (int i = 0; i < concurrentUsers; i++)
            {
                tasks.Add(TestConcurrentPriorityCalculations($"user{i}@test.com", calculationsPerUser));
            }

            var results = await Task.WhenAll(tasks);
            var summary = AnalyzeResults(results, "Priority Calculation Load Test");

            _output.WriteLine($"Priority Calculation Load Test Results:");
            _output.WriteLine($"- Total Calculations: {summary.TotalRequests}");
            _output.WriteLine($"- Successful Calculations: {summary.SuccessfulRequests}");
            _output.WriteLine($"- Failed Calculations: {summary.FailedRequests}");
            _output.WriteLine($"- Average Response Time: {summary.AverageResponseTime}ms");
            _output.WriteLine($"- 95th Percentile: {summary.Percentile95}ms");
            _output.WriteLine($"- Throughput: {summary.Throughput} calc/sec");

            // Assertions
            Assert.True(summary.SuccessRate >= 0.95, $"Success rate {summary.SuccessRate:P} is below 95%");
            Assert.True(summary.AverageResponseTime <= 2000, $"Average response time {summary.AverageResponseTime}ms exceeds 2000ms");
        }

        [Fact]
        public async Task LoadTest_Analytics_ConcurrentQueries()
        {
            // Test concurrent analytics queries
            const int concurrentUsers = 20;
            const int queriesPerUser = 10;
            
            var tasks = new List<Task<LoadTestResult>>();
            
            for (int i = 0; i < concurrentUsers; i++)
            {
                tasks.Add(TestConcurrentAnalyticsQueries($"user{i}@test.com", queriesPerUser));
            }

            var results = await Task.WhenAll(tasks);
            var summary = AnalyzeResults(results, "Analytics Query Load Test");

            _output.WriteLine($"Analytics Query Load Test Results:");
            _output.WriteLine($"- Total Queries: {summary.TotalRequests}");
            _output.WriteLine($"- Successful Queries: {summary.SuccessfulRequests}");
            _output.WriteLine($"- Failed Queries: {summary.FailedRequests}");
            _output.WriteLine($"- Average Response Time: {summary.AverageResponseTime}ms");
            _output.WriteLine($"- 95th Percentile: {summary.Percentile95}ms");
            _output.WriteLine($"- Throughput: {summary.Throughput} queries/sec");

            // Assertions
            Assert.True(summary.SuccessRate >= 0.90, $"Success rate {summary.SuccessRate:P} is below 90%");
            Assert.True(summary.AverageResponseTime <= 3000, $"Average response time {summary.AverageResponseTime}ms exceeds 3000ms");
        }

        #endregion

        #region Stress Tests

        [Fact]
        public async Task StressTest_HighConcurrency_Authentication()
        {
            // Stress test with high concurrency
            const int concurrentUsers = 200;
            const int requestsPerUser = 5;
            
            var tasks = new List<Task<LoadTestResult>>();
            
            for (int i = 0; i < concurrentUsers; i++)
            {
                tasks.Add(TestConcurrentAuthentication($"stressuser{i}@test.com", requestsPerUser));
            }

            var results = await Task.WhenAll(tasks);
            var summary = AnalyzeResults(results, "High Concurrency Stress Test");

            _output.WriteLine($"High Concurrency Stress Test Results:");
            _output.WriteLine($"- Total Requests: {summary.TotalRequests}");
            _output.WriteLine($"- Successful Requests: {summary.SuccessfulRequests}");
            _output.WriteLine($"- Failed Requests: {summary.FailedRequests}");
            _output.WriteLine($"- Average Response Time: {summary.AverageResponseTime}ms");
            _output.WriteLine($"- 95th Percentile: {summary.Percentile95}ms");
            _output.WriteLine($"- Throughput: {summary.Throughput} req/sec");

            // Stress test assertions (more lenient)
            Assert.True(summary.SuccessRate >= 0.80, $"Success rate {summary.SuccessRate:P} is below 80%");
            Assert.True(summary.AverageResponseTime <= 5000, $"Average response time {summary.AverageResponseTime}ms exceeds 5000ms");
        }

        [Fact]
        public async Task StressTest_DatabaseIntensive_Operations()
        {
            // Stress test database-intensive operations
            const int concurrentUsers = 100;
            const int operationsPerUser = 10;
            
            var tasks = new List<Task<LoadTestResult>>();
            
            for (int i = 0; i < concurrentUsers; i++)
            {
                tasks.Add(TestConcurrentWorkRequestOperations($"dbstressuser{i}@test.com", operationsPerUser));
            }

            var results = await Task.WhenAll(tasks);
            var summary = AnalyzeResults(results, "Database Intensive Stress Test");

            _output.WriteLine($"Database Intensive Stress Test Results:");
            _output.WriteLine($"- Total Operations: {summary.TotalRequests}");
            _output.WriteLine($"- Successful Operations: {summary.SuccessfulRequests}");
            _output.WriteLine($"- Failed Operations: {summary.FailedRequests}");
            _output.WriteLine($"- Average Response Time: {summary.AverageResponseTime}ms");
            _output.WriteLine($"- 95th Percentile: {summary.Percentile95}ms");
            _output.WriteLine($"- Throughput: {summary.Throughput} ops/sec");

            // Stress test assertions
            Assert.True(summary.SuccessRate >= 0.85, $"Success rate {summary.SuccessRate:P} is below 85%");
            Assert.True(summary.AverageResponseTime <= 10000, $"Average response time {summary.AverageResponseTime}ms exceeds 10000ms");
        }

        #endregion

        #region Scalability Tests

        [Fact]
        public async Task ScalabilityTest_CachePerformance()
        {
            // Test cache performance under load
            const int concurrentUsers = 50;
            const int cacheOperationsPerUser = 20;
            
            var tasks = new List<Task<LoadTestResult>>();
            
            for (int i = 0; i < concurrentUsers; i++)
            {
                tasks.Add(TestConcurrentCacheOperations($"cacheuser{i}@test.com", cacheOperationsPerUser));
            }

            var results = await Task.WhenAll(tasks);
            var summary = AnalyzeResults(results, "Cache Performance Scalability Test");

            _output.WriteLine($"Cache Performance Scalability Test Results:");
            _output.WriteLine($"- Total Cache Operations: {summary.TotalRequests}");
            _output.WriteLine($"- Successful Operations: {summary.SuccessfulRequests}");
            _output.WriteLine($"- Failed Operations: {summary.FailedRequests}");
            _output.WriteLine($"- Average Response Time: {summary.AverageResponseTime}ms");
            _output.WriteLine($"- 95th Percentile: {summary.Percentile95}ms");
            _output.WriteLine($"- Throughput: {summary.Throughput} ops/sec");

            // Cache performance should be very fast
            Assert.True(summary.SuccessRate >= 0.98, $"Success rate {summary.SuccessRate:P} is below 98%");
            Assert.True(summary.AverageResponseTime <= 100, $"Average response time {summary.AverageResponseTime}ms exceeds 100ms");
        }

        [Fact]
        public async Task ScalabilityTest_WorkflowEngine()
        {
            // Test workflow engine scalability
            const int concurrentUsers = 40;
            const int workflowOperationsPerUser = 15;
            
            var tasks = new List<Task<LoadTestResult>>();
            
            for (int i = 0; i < concurrentUsers; i++)
            {
                tasks.Add(TestConcurrentWorkflowOperations($"workflowuser{i}@test.com", workflowOperationsPerUser));
            }

            var results = await Task.WhenAll(tasks);
            var summary = AnalyzeResults(results, "Workflow Engine Scalability Test");

            _output.WriteLine($"Workflow Engine Scalability Test Results:");
            _output.WriteLine($"- Total Workflow Operations: {summary.TotalRequests}");
            _output.WriteLine($"- Successful Operations: {summary.SuccessfulRequests}");
            _output.WriteLine($"- Failed Operations: {summary.FailedRequests}");
            _output.WriteLine($"- Average Response Time: {summary.AverageResponseTime}ms");
            _output.WriteLine($"- 95th Percentile: {summary.Percentile95}ms");
            _output.WriteLine($"- Throughput: {summary.Throughput} ops/sec");

            // Workflow operations should be reliable
            Assert.True(summary.SuccessRate >= 0.95, $"Success rate {summary.SuccessRate:P} is below 95%");
            Assert.True(summary.AverageResponseTime <= 2000, $"Average response time {summary.AverageResponseTime}ms exceeds 2000ms");
        }

        #endregion

        #region Helper Methods

        private async Task<LoadTestResult> TestConcurrentAuthentication(string email, int requestCount)
        {
            var results = new List<RequestResult>();
            var random = new Random();

            for (int i = 0; i < requestCount; i++)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                try
                {
                    // Register user
                    var registerRequest = new
                    {
                        Email = email,
                        Password = "TestPassword123!",
                        FirstName = "Test",
                        LastName = "User",
                        DepartmentId = 1
                    };

                    var registerJson = JsonSerializer.Serialize(registerRequest);
                    var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");
                    
                    var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
                    stopwatch.Stop();

                    results.Add(new RequestResult
                    {
                        Success = registerResponse.IsSuccessStatusCode,
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        StatusCode = (int)registerResponse.StatusCode
                    });

                    // Login user
                    stopwatch.Restart();
                    var loginRequest = new
                    {
                        Email = email,
                        Password = "TestPassword123!"
                    };

                    var loginJson = JsonSerializer.Serialize(loginRequest);
                    var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
                    
                    var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
                    stopwatch.Stop();

                    results.Add(new RequestResult
                    {
                        Success = loginResponse.IsSuccessStatusCode,
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        StatusCode = (int)loginResponse.StatusCode
                    });
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    results.Add(new RequestResult
                    {
                        Success = false,
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        StatusCode = 0,
                        Error = ex.Message
                    });
                }

                // Small delay between requests
                await Task.Delay(random.Next(10, 50));
            }

            return new LoadTestResult
            {
                Results = results,
                TestName = "Authentication",
                UserEmail = email
            };
        }

        private async Task<LoadTestResult> TestConcurrentWorkRequestOperations(string email, int operationCount)
        {
            var results = new List<RequestResult>();
            var random = new Random();

            // First authenticate
            var authResult = await AuthenticateUser(email);
            if (!authResult.Success)
            {
                return new LoadTestResult
                {
                    Results = new List<RequestResult> { authResult },
                    TestName = "Work Request Operations",
                    UserEmail = email
                };
            }

            for (int i = 0; i < operationCount; i++)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                try
                {
                    // Create work request
                    var workRequest = new
                    {
                        Title = $"Load Test Work Request {i}",
                        Description = $"Load test work request description {i}",
                        BusinessValue = random.Next(1, 10),
                        Urgency = random.Next(1, 5),
                        DepartmentId = 1,
                        BusinessVerticalId = 1,
                        EstimatedHours = random.Next(1, 100)
                    };

                    var json = JsonSerializer.Serialize(workRequest);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    var response = await _client.PostAsync("/api/workrequests", content);
                    stopwatch.Stop();

                    results.Add(new RequestResult
                    {
                        Success = response.IsSuccessStatusCode,
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        StatusCode = (int)response.StatusCode
                    });
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    results.Add(new RequestResult
                    {
                        Success = false,
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        StatusCode = 0,
                        Error = ex.Message
                    });
                }

                // Small delay between operations
                await Task.Delay(random.Next(20, 100));
            }

            return new LoadTestResult
            {
                Results = results,
                TestName = "Work Request Operations",
                UserEmail = email
            };
        }

        private async Task<LoadTestResult> TestConcurrentPriorityCalculations(string email, int calculationCount)
        {
            var results = new List<RequestResult>();
            var random = new Random();

            // First authenticate
            var authResult = await AuthenticateUser(email);
            if (!authResult.Success)
            {
                return new LoadTestResult
                {
                    Results = new List<RequestResult> { authResult },
                    TestName = "Priority Calculations",
                    UserEmail = email
                };
            }

            for (int i = 0; i < calculationCount; i++)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                try
                {
                    var response = await _client.GetAsync("/api/analytics/priority-predictions");
                    stopwatch.Stop();

                    results.Add(new RequestResult
                    {
                        Success = response.IsSuccessStatusCode,
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        StatusCode = (int)response.StatusCode
                    });
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    results.Add(new RequestResult
                    {
                        Success = false,
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        StatusCode = 0,
                        Error = ex.Message
                    });
                }

                // Small delay between calculations
                await Task.Delay(random.Next(50, 200));
            }

            return new LoadTestResult
            {
                Results = results,
                TestName = "Priority Calculations",
                UserEmail = email
            };
        }

        private async Task<LoadTestResult> TestConcurrentAnalyticsQueries(string email, int queryCount)
        {
            var results = new List<RequestResult>();
            var random = new Random();
            var analyticsEndpoints = new[]
            {
                "/api/analytics/executive-dashboard",
                "/api/analytics/department-dashboard",
                "/api/analytics/workload-forecast",
                "/api/analytics/priority-trends",
                "/api/analytics/completion-predictions"
            };

            // First authenticate
            var authResult = await AuthenticateUser(email);
            if (!authResult.Success)
            {
                return new LoadTestResult
                {
                    Results = new List<RequestResult> { authResult },
                    TestName = "Analytics Queries",
                    UserEmail = email
                };
            }

            for (int i = 0; i < queryCount; i++)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                try
                {
                    var endpoint = analyticsEndpoints[random.Next(analyticsEndpoints.Length)];
                    var response = await _client.GetAsync(endpoint);
                    stopwatch.Stop();

                    results.Add(new RequestResult
                    {
                        Success = response.IsSuccessStatusCode,
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        StatusCode = (int)response.StatusCode
                    });
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    results.Add(new RequestResult
                    {
                        Success = false,
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        StatusCode = 0,
                        Error = ex.Message
                    });
                }

                // Small delay between queries
                await Task.Delay(random.Next(100, 300));
            }

            return new LoadTestResult
            {
                Results = results,
                TestName = "Analytics Queries",
                UserEmail = email
            };
        }

        private async Task<LoadTestResult> TestConcurrentCacheOperations(string email, int operationCount)
        {
            var results = new List<RequestResult>();
            var random = new Random();

            // First authenticate
            var authResult = await AuthenticateUser(email);
            if (!authResult.Success)
            {
                return new LoadTestResult
                {
                    Results = new List<RequestResult> { authResult },
                    TestName = "Cache Operations",
                    UserEmail = email
                };
            }

            for (int i = 0; i < operationCount; i++)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                try
                {
                    // Test cached endpoints
                    var endpoints = new[]
                    {
                        "/api/workrequests",
                        "/api/departments",
                        "/api/businessverticals",
                        "/api/analytics/executive-dashboard"
                    };

                    var endpoint = endpoints[random.Next(endpoints.Length)];
                    var response = await _client.GetAsync(endpoint);
                    stopwatch.Stop();

                    results.Add(new RequestResult
                    {
                        Success = response.IsSuccessStatusCode,
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        StatusCode = (int)response.StatusCode
                    });
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    results.Add(new RequestResult
                    {
                        Success = false,
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        StatusCode = 0,
                        Error = ex.Message
                    });
                }

                // Very small delay for cache operations
                await Task.Delay(random.Next(5, 25));
            }

            return new LoadTestResult
            {
                Results = results,
                TestName = "Cache Operations",
                UserEmail = email
            };
        }

        private async Task<LoadTestResult> TestConcurrentWorkflowOperations(string email, int operationCount)
        {
            var results = new List<RequestResult>();
            var random = new Random();

            // First authenticate
            var authResult = await AuthenticateUser(email);
            if (!authResult.Success)
            {
                return new LoadTestResult
                {
                    Results = new List<RequestResult> { authResult },
                    TestName = "Workflow Operations",
                    UserEmail = email
                };
            }

            for (int i = 0; i < operationCount; i++)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                try
                {
                    // Test workflow-related endpoints
                    var endpoints = new[]
                    {
                        "/api/workflow/stages",
                        "/api/workflow/transitions",
                        "/api/workflow/configuration",
                        "/api/workflow/analytics"
                    };

                    var endpoint = endpoints[random.Next(endpoints.Length)];
                    var response = await _client.GetAsync(endpoint);
                    stopwatch.Stop();

                    results.Add(new RequestResult
                    {
                        Success = response.IsSuccessStatusCode,
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        StatusCode = (int)response.StatusCode
                    });
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    results.Add(new RequestResult
                    {
                        Success = false,
                        ResponseTime = stopwatch.ElapsedMilliseconds,
                        StatusCode = 0,
                        Error = ex.Message
                    });
                }

                // Small delay between operations
                await Task.Delay(random.Next(30, 150));
            }

            return new LoadTestResult
            {
                Results = results,
                TestName = "Workflow Operations",
                UserEmail = email
            };
        }

        private async Task<RequestResult> AuthenticateUser(string email)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
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

        private LoadTestSummary AnalyzeResults(LoadTestResult[] results, string testName)
        {
            var allResults = new List<RequestResult>();
            foreach (var result in results)
            {
                allResults.AddRange(result.Results);
            }

            var successfulResults = allResults.Where(r => r.Success).ToList();
            var responseTimes = successfulResults.Select(r => r.ResponseTime).OrderBy(t => t).ToList();

            return new LoadTestSummary
            {
                TestName = testName,
                TotalRequests = allResults.Count,
                SuccessfulRequests = successfulResults.Count,
                FailedRequests = allResults.Count - successfulResults.Count,
                SuccessRate = (double)successfulResults.Count / allResults.Count,
                AverageResponseTime = successfulResults.Any() ? successfulResults.Average(r => r.ResponseTime) : 0,
                MinResponseTime = responseTimes.Any() ? responseTimes.First() : 0,
                MaxResponseTime = responseTimes.Any() ? responseTimes.Last() : 0,
                Percentile95 = responseTimes.Any() ? responseTimes[(int)(responseTimes.Count * 0.95)] : 0,
                Percentile99 = responseTimes.Any() ? responseTimes[(int)(responseTimes.Count * 0.99)] : 0,
                Throughput = allResults.Count > 0 ? allResults.Count / (allResults.Max(r => r.ResponseTime) / 1000.0) : 0
            };
        }

        #endregion
    }

    #region Data Models

    public class LoadTestResult
    {
        public List<RequestResult> Results { get; set; } = new();
        public string TestName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }

    public class RequestResult
    {
        public bool Success { get; set; }
        public long ResponseTime { get; set; }
        public int StatusCode { get; set; }
        public string Error { get; set; } = string.Empty;
    }

    public class LoadTestSummary
    {
        public string TestName { get; set; } = string.Empty;
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

    #endregion
}
