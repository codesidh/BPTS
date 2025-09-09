using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace WorkIntakeSystem.Tests.LoadTesting
{
    /// <summary>
    /// Load test fixture for setting up the test environment
    /// </summary>
    public class LoadTestFixture : WebApplicationFactory<Program>, IDisposable
    {
        private bool _disposed = false;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Configure for load testing
                services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
                {
                    options.Limits.MaxConcurrentConnections = 1000;
                    options.Limits.MaxConcurrentUpgradedConnections = 1000;
                    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
                });

                // Configure logging for load tests
                services.AddLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Warning);
                });

                // Configure HttpClient for load testing
                services.Configure<HttpClientFactoryOptions>(options =>
                {
                    options.HandlerLifetime = TimeSpan.FromMinutes(10);
                });
            });

            builder.UseEnvironment("Testing");
        }

        public HttpClient CreateClient()
        {
            var client = base.CreateClient();
            
            // Configure client for load testing
            client.Timeout = TimeSpan.FromMinutes(5);
            
            // Add headers for load testing identification
            client.DefaultRequestHeaders.Add("X-Load-Test", "true");
            client.DefaultRequestHeaders.Add("User-Agent", "LoadTestRunner/1.0");
            
            return client;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    base.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
