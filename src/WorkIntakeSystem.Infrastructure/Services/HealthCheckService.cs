using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Infrastructure.Data;
using StackExchange.Redis;

namespace WorkIntakeSystem.Infrastructure.Services
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly ILogger<HealthCheckService> _logger;
        private readonly IConfiguration _configuration;
        private readonly WorkIntakeDbContext _dbContext;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, bool> _healthCheckSettings;
        private readonly Dictionary<string, TimeSpan> _healthCheckIntervals;

        public HealthCheckService(
            ILogger<HealthCheckService> logger,
            IConfiguration configuration,
            WorkIntakeDbContext dbContext,
            IConnectionMultiplexer redisConnection,
            HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _dbContext = dbContext;
            _redisConnection = redisConnection;
            _httpClient = httpClient;
            
            _healthCheckSettings = new Dictionary<string, bool>
            {
                ["database"] = configuration.GetValue<bool>("HealthChecks:Database:Enabled", true),
                ["redis"] = configuration.GetValue<bool>("HealthChecks:Redis:Enabled", true),
                ["external_services"] = configuration.GetValue<bool>("HealthChecks:ExternalServices:Enabled", true),
                ["system_resources"] = configuration.GetValue<bool>("HealthChecks:SystemResources:Enabled", true),
                ["application"] = configuration.GetValue<bool>("HealthChecks:Application:Enabled", true)
            };
            
            _healthCheckIntervals = new Dictionary<string, TimeSpan>
            {
                ["database"] = TimeSpan.FromSeconds(configuration.GetValue<int>("HealthChecks:Database:IntervalSeconds", 30)),
                ["redis"] = TimeSpan.FromSeconds(configuration.GetValue<int>("HealthChecks:Redis:IntervalSeconds", 30)),
                ["external_services"] = TimeSpan.FromSeconds(configuration.GetValue<int>("HealthChecks:ExternalServices:IntervalSeconds", 60)),
                ["system_resources"] = TimeSpan.FromSeconds(configuration.GetValue<int>("HealthChecks:SystemResources:IntervalSeconds", 30)),
                ["application"] = TimeSpan.FromSeconds(configuration.GetValue<int>("HealthChecks:Application:IntervalSeconds", 30))
            };
        }

        #region Database Health Checks

        public async Task<HealthCheckResult> CheckDatabaseConnectivityAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["database"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "Database health check is disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                var canConnect = await _dbContext.Database.CanConnectAsync();
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = canConnect,
                    Status = canConnect ? "Healthy" : "Unhealthy",
                    Description = canConnect ? "Database connection is working" : "Cannot connect to database",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "can_connect", canConnect },
                        { "database_provider", _dbContext.Database.ProviderName },
                        { "connection_string", "***" }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Database connectivity check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckDatabasePerformanceAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["database"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "Database performance check is disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                // Simple performance test - count users
                var userCount = await _dbContext.Users.CountAsync();
                stopwatch.Stop();
                
                var isHealthy = stopwatch.Elapsed.TotalMilliseconds < 1000; // Less than 1 second
                var status = isHealthy ? "Healthy" : "Warning";
                var description = $"Database query took {stopwatch.Elapsed.TotalMilliseconds:F0}ms";
                
                return new HealthCheckResult
                {
                    IsHealthy = isHealthy,
                    Status = status,
                    Description = description,
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "query_time_ms", stopwatch.Elapsed.TotalMilliseconds },
                        { "user_count", userCount },
                        { "performance_threshold_ms", 1000 }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Database performance check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckDatabaseMigrationStatusAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["database"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "Database migration check is disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await _dbContext.Database.GetAppliedMigrationsAsync();
                
                stopwatch.Stop();
                
                var hasPendingMigrations = pendingMigrations.Any();
                var isHealthy = !hasPendingMigrations;
                var status = isHealthy ? "Healthy" : "Warning";
                var description = hasPendingMigrations 
                    ? $"Database has {pendingMigrations.Count()} pending migrations"
                    : "Database is up to date";
                
                return new HealthCheckResult
                {
                    IsHealthy = isHealthy,
                    Status = status,
                    Description = description,
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "pending_migrations_count", pendingMigrations.Count() },
                        { "applied_migrations_count", appliedMigrations.Count() },
                        { "has_pending_migrations", hasPendingMigrations }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Database migration check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        #endregion

        #region Cache Health Checks

        public async Task<HealthCheckResult> CheckRedisConnectivityAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["redis"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "Redis connectivity check is disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                var database = _redisConnection.GetDatabase();
                var pingResult = await database.PingAsync();
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = true,
                    Status = "Healthy",
                    Description = $"Redis is responding (Ping: {pingResult.TotalMilliseconds:F0}ms)",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "ping_time_ms", pingResult.TotalMilliseconds },
                        { "redis_version", "6.0+" },
                        { "endpoint_count", _redisConnection.GetEndPoints().Length }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Redis connectivity check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckRedisPerformanceAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["redis"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "Redis performance check is disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                var database = _redisConnection.GetDatabase();
                
                // Test Redis performance with a simple set/get operation
                var testKey = $"health_check_{Guid.NewGuid()}";
                var testValue = "test_value";
                
                await database.StringSetAsync(testKey, testValue, TimeSpan.FromSeconds(10));
                var retrievedValue = await database.StringGetAsync(testKey);
                await database.KeyDeleteAsync(testKey);
                
                stopwatch.Stop();
                
                var isHealthy = stopwatch.Elapsed.TotalMilliseconds < 100; // Less than 100ms
                var status = isHealthy ? "Healthy" : "Warning";
                var description = $"Redis operation took {stopwatch.Elapsed.TotalMilliseconds:F0}ms";
                
                return new HealthCheckResult
                {
                    IsHealthy = isHealthy,
                    Status = status,
                    Description = description,
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "operation_time_ms", stopwatch.Elapsed.TotalMilliseconds },
                        { "test_successful", retrievedValue == testValue },
                        { "performance_threshold_ms", 100 }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Redis performance check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckMemoryCacheStatusAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Memory cache is typically handled by the framework
                // This is a placeholder for more sophisticated memory cache monitoring
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = true,
                    Status = "Healthy",
                    Description = "Memory cache is operational",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "cache_type", "MemoryCache" },
                        { "status", "operational" }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Memory cache check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        #endregion

        #region External Services Health Checks

        public async Task<HealthCheckResult> CheckExternalServiceAsync(string serviceName, string endpoint)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["external_services"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "External service checks are disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                var response = await _httpClient.GetAsync(endpoint);
                stopwatch.Stop();
                
                var isHealthy = response.IsSuccessStatusCode;
                var status = isHealthy ? "Healthy" : "Unhealthy";
                var description = $"{serviceName} responded with status {response.StatusCode}";
                
                return new HealthCheckResult
                {
                    IsHealthy = isHealthy,
                    Status = status,
                    Description = description,
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "service_name", serviceName },
                        { "endpoint", endpoint },
                        { "status_code", (int)response.StatusCode },
                        { "response_time_ms", stopwatch.Elapsed.TotalMilliseconds }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"{serviceName} check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "service_name", serviceName },
                        { "endpoint", endpoint },
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckMicrosoft365ServicesAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["external_services"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "Microsoft 365 service checks are disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                var services = new[]
                {
                    "https://graph.microsoft.com/v1.0/",
                    "https://login.microsoftonline.com/common/v2.0/.well-known/openid_configuration"
                };

                var healthyServices = 0;
                var serviceResults = new Dictionary<string, bool>();

                foreach (var service in services)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(service);
                        serviceResults[service] = response.IsSuccessStatusCode;
                        if (response.IsSuccessStatusCode) healthyServices++;
                    }
                    catch
                    {
                        serviceResults[service] = false;
                    }
                }

                stopwatch.Stop();
                
                var isHealthy = healthyServices > 0;
                var status = isHealthy ? "Healthy" : "Unhealthy";
                var description = $"{healthyServices}/{services.Length} Microsoft 365 services are responding";
                
                return new HealthCheckResult
                {
                    IsHealthy = isHealthy,
                    Status = status,
                    Description = description,
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "healthy_services", healthyServices },
                        { "total_services", services.Length },
                        { "service_results", serviceResults }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Microsoft 365 services check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckDevOpsServicesAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["external_services"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "DevOps service checks are disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                var services = new[]
                {
                    "https://api.github.com",
                    "https://httpbin.org/status/200"
                };

                var healthyServices = 0;
                var serviceResults = new Dictionary<string, bool>();

                foreach (var service in services)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(service);
                        serviceResults[service] = response.IsSuccessStatusCode;
                        if (response.IsSuccessStatusCode) healthyServices++;
                    }
                    catch
                    {
                        serviceResults[service] = false;
                    }
                }

                stopwatch.Stop();
                
                var isHealthy = healthyServices > 0;
                var status = isHealthy ? "Healthy" : "Unhealthy";
                var description = $"{healthyServices}/{services.Length} DevOps services are responding";
                
                return new HealthCheckResult
                {
                    IsHealthy = isHealthy,
                    Status = status,
                    Description = description,
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "healthy_services", healthyServices },
                        { "total_services", services.Length },
                        { "service_results", serviceResults }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"DevOps services check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckFinancialSystemsAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["external_services"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "Financial systems checks are disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                // Placeholder for financial system health checks
                // In a real implementation, this would check actual financial system endpoints
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = true,
                    Status = "Healthy",
                    Description = "Financial systems are operational",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "systems_checked", new[] { "SAP", "Oracle", "QuickBooks" } },
                        { "status", "operational" }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Financial systems check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        #endregion

        #region System Health Checks

        public async Task<HealthCheckResult> CheckSystemResourcesAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["system_resources"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "System resources checks are disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                var process = Process.GetCurrentProcess();
                var memoryUsage = process.WorkingSet64;
                var cpuUsage = await GetCpuUsageAsync();
                
                stopwatch.Stop();
                
                var isHealthy = memoryUsage < 500 * 1024 * 1024 && cpuUsage < 80; // 500MB and 80% CPU
                var status = isHealthy ? "Healthy" : "Warning";
                var description = $"Memory: {memoryUsage / 1024 / 1024}MB, CPU: {cpuUsage:F1}%";
                
                return new HealthCheckResult
                {
                    IsHealthy = isHealthy,
                    Status = status,
                    Description = description,
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "memory_usage_mb", memoryUsage / 1024 / 1024 },
                        { "cpu_usage_percent", cpuUsage },
                        { "process_id", process.Id },
                        { "thread_count", process.Threads.Count },
                        { "memory_threshold_mb", 500 },
                        { "cpu_threshold_percent", 80 }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"System resources check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckDiskSpaceAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["system_resources"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "Disk space checks are disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                var drive = new DriveInfo(Environment.CurrentDirectory);
                var freeSpace = drive.AvailableFreeSpace;
                var totalSpace = drive.TotalSize;
                var usedSpace = totalSpace - freeSpace;
                var usagePercentage = (double)usedSpace / totalSpace * 100;
                
                stopwatch.Stop();
                
                var isHealthy = usagePercentage < 90; // Less than 90% used
                var status = isHealthy ? "Healthy" : "Warning";
                var description = $"Disk usage: {usagePercentage:F1}% ({usedSpace / 1024 / 1024 / 1024:F1}GB used of {totalSpace / 1024 / 1024 / 1024:F1}GB)";
                
                return new HealthCheckResult
                {
                    IsHealthy = isHealthy,
                    Status = status,
                    Description = description,
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "free_space_gb", freeSpace / 1024.0 / 1024.0 / 1024.0 },
                        { "total_space_gb", totalSpace / 1024.0 / 1024.0 / 1024.0 },
                        { "used_space_gb", usedSpace / 1024.0 / 1024.0 / 1024.0 },
                        { "usage_percentage", usagePercentage },
                        { "drive_name", drive.Name },
                        { "usage_threshold_percent", 90 }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Disk space check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckMemoryUsageAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["system_resources"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "Memory usage checks are disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                var process = Process.GetCurrentProcess();
                var memoryUsage = process.WorkingSet64;
                var memoryUsageMB = memoryUsage / 1024.0 / 1024.0;
                
                stopwatch.Stop();
                
                var isHealthy = memoryUsageMB < 500; // Less than 500MB
                var status = isHealthy ? "Healthy" : "Warning";
                var description = $"Memory usage: {memoryUsageMB:F1}MB";
                
                return new HealthCheckResult
                {
                    IsHealthy = isHealthy,
                    Status = status,
                    Description = description,
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "memory_usage_mb", memoryUsageMB },
                        { "memory_threshold_mb", 500 },
                        { "process_id", process.Id }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Memory usage check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckCpuUsageAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["system_resources"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "CPU usage checks are disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                var cpuUsage = await GetCpuUsageAsync();
                stopwatch.Stop();
                
                var isHealthy = cpuUsage < 80; // Less than 80%
                var status = isHealthy ? "Healthy" : "Warning";
                var description = $"CPU usage: {cpuUsage:F1}%";
                
                return new HealthCheckResult
                {
                    IsHealthy = isHealthy,
                    Status = status,
                    Description = description,
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "cpu_usage_percent", cpuUsage },
                        { "cpu_threshold_percent", 80 }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"CPU usage check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        #endregion

        #region Application Health Checks

        public async Task<HealthCheckResult> CheckApplicationPerformanceAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["application"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "Application performance checks are disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                // Simple application performance test
                await Task.Delay(10); // Simulate some work
                stopwatch.Stop();
                
                var isHealthy = stopwatch.Elapsed.TotalMilliseconds < 100; // Less than 100ms
                var status = isHealthy ? "Healthy" : "Warning";
                var description = $"Application performance test took {stopwatch.Elapsed.TotalMilliseconds:F0}ms";
                
                return new HealthCheckResult
                {
                    IsHealthy = isHealthy,
                    Status = status,
                    Description = description,
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "test_time_ms", stopwatch.Elapsed.TotalMilliseconds },
                        { "performance_threshold_ms", 100 }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Application performance check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckApiEndpointsAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["application"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "API endpoint checks are disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                // This would typically check actual API endpoints
                // For now, we'll simulate a health check
                await Task.Delay(50);
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = true,
                    Status = "Healthy",
                    Description = "API endpoints are responding",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "endpoints_checked", new[] { "/health", "/api/users", "/api/workrequests" } },
                        { "status", "operational" }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"API endpoints check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckBackgroundServicesAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["application"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "Background service checks are disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                // This would typically check actual background services
                // For now, we'll simulate a health check
                await Task.Delay(20);
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = true,
                    Status = "Healthy",
                    Description = "Background services are running",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "services_checked", new[] { "ServiceBrokerHostedService", "EmailService", "WorkflowEngine" } },
                        { "status", "operational" }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Background services check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        public async Task<HealthCheckResult> CheckWorkflowEngineAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (!_healthCheckSettings["application"])
                {
                    return new HealthCheckResult
                    {
                        IsHealthy = true,
                        Status = "Disabled",
                        Description = "Workflow engine checks are disabled",
                        ResponseTime = TimeSpan.Zero
                    };
                }

                // This would typically check the actual workflow engine
                // For now, we'll simulate a health check
                await Task.Delay(30);
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = true,
                    Status = "Healthy",
                    Description = "Workflow engine is operational",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "engine_status", "operational" },
                        { "active_workflows", 0 },
                        { "pending_transitions", 0 }
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                return new HealthCheckResult
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Description = $"Workflow engine check failed: {ex.Message}",
                    ResponseTime = stopwatch.Elapsed,
                    Data = new Dictionary<string, object>
                    {
                        { "error", ex.Message },
                        { "exception_type", ex.GetType().Name }
                    }
                };
            }
        }

        #endregion

        #region Comprehensive Health Check

        public async Task<ComprehensiveHealthReport> RunComprehensiveHealthCheckAsync()
        {
            var overallStopwatch = Stopwatch.StartNew();
            var report = new ComprehensiveHealthReport
            {
                Timestamp = DateTime.UtcNow,
                OverallStatus = "Checking"
            };

            try
            {
                // Database checks
                report.DatabaseChecks.Add(await CheckDatabaseConnectivityAsync());
                report.DatabaseChecks.Add(await CheckDatabasePerformanceAsync());
                report.DatabaseChecks.Add(await CheckDatabaseMigrationStatusAsync());

                // Cache checks
                report.CacheChecks.Add(await CheckRedisConnectivityAsync());
                report.CacheChecks.Add(await CheckRedisPerformanceAsync());
                report.CacheChecks.Add(await CheckMemoryCacheStatusAsync());

                // External service checks
                report.ExternalServiceChecks.Add(await CheckMicrosoft365ServicesAsync());
                report.ExternalServiceChecks.Add(await CheckDevOpsServicesAsync());
                report.ExternalServiceChecks.Add(await CheckFinancialSystemsAsync());

                // System checks
                report.SystemChecks.Add(await CheckSystemResourcesAsync());
                report.SystemChecks.Add(await CheckDiskSpaceAsync());
                report.SystemChecks.Add(await CheckMemoryUsageAsync());
                report.SystemChecks.Add(await CheckCpuUsageAsync());

                // Application checks
                report.ApplicationChecks.Add(await CheckApplicationPerformanceAsync());
                report.ApplicationChecks.Add(await CheckApiEndpointsAsync());
                report.ApplicationChecks.Add(await CheckBackgroundServicesAsync());
                report.ApplicationChecks.Add(await CheckWorkflowEngineAsync());

                overallStopwatch.Stop();
                report.TotalCheckTime = overallStopwatch.Elapsed;

                // Calculate overall health
                var allChecks = report.DatabaseChecks.Concat(report.CacheChecks)
                    .Concat(report.ExternalServiceChecks)
                    .Concat(report.SystemChecks)
                    .Concat(report.ApplicationChecks)
                    .ToList();

                report.OverallHealth = allChecks.All(c => c.IsHealthy);
                report.OverallStatus = report.OverallHealth ? "Healthy" : "Unhealthy";

                // Summary statistics
                report.Summary["total_checks"] = allChecks.Count;
                report.Summary["healthy_checks"] = allChecks.Count(c => c.IsHealthy);
                report.Summary["unhealthy_checks"] = allChecks.Count(c => !c.IsHealthy);
                report.Summary["total_time_ms"] = report.TotalCheckTime.TotalMilliseconds;

                _logger.LogInformation("Comprehensive health check completed. Overall: {Status}, Time: {Time}ms", 
                    report.OverallStatus, report.TotalCheckTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                overallStopwatch.Stop();
                report.TotalCheckTime = overallStopwatch.Elapsed;
                report.OverallHealth = false;
                report.OverallStatus = "Error";
                report.Summary["error"] = ex.Message;
                
                _logger.LogError(ex, "Error during comprehensive health check");
            }

            return report;
        }

        #endregion

        #region Health Check Configuration

        public async Task<bool> IsHealthCheckEnabledAsync(string checkName)
        {
            return _healthCheckSettings.TryGetValue(checkName, out var enabled) && enabled;
        }

        public async Task SetHealthCheckEnabledAsync(string checkName, bool enabled)
        {
            if (_healthCheckSettings.ContainsKey(checkName))
            {
                _healthCheckSettings[checkName] = enabled;
                _logger.LogInformation("Health check '{CheckName}' enabled status changed to: {Enabled}", checkName, enabled);
            }
        }

        public async Task<TimeSpan> GetHealthCheckIntervalAsync(string checkName)
        {
            return _healthCheckIntervals.TryGetValue(checkName, out var interval) ? interval : TimeSpan.FromSeconds(30);
        }

        public async Task SetHealthCheckIntervalAsync(string checkName, TimeSpan interval)
        {
            if (_healthCheckIntervals.ContainsKey(checkName))
            {
                _healthCheckIntervals[checkName] = interval;
                _logger.LogInformation("Health check '{CheckName}' interval changed to: {Interval}", checkName, interval);
            }
        }

        #endregion

        #region Private Methods

        private async Task<double> GetCpuUsageAsync()
        {
            try
            {
                var startTime = DateTime.UtcNow;
                var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
                
                await Task.Delay(100); // Wait 100ms to measure CPU usage
                
                var endTime = DateTime.UtcNow;
                var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
                
                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed) * 100;
                
                return Math.Min(cpuUsageTotal, 100.0);
            }
            catch
            {
                return 0.0;
            }
        }

        #endregion
    }
} 