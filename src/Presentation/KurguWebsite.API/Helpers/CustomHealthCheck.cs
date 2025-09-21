using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace KurguWebsite.WebAPI.Helpers
{
    public class CustomHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CustomHealthCheck> _logger;

        public CustomHealthCheck(IConfiguration configuration, ILogger<CustomHealthCheck> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check various system components
                var isHealthy = true;
                var data = new Dictionary<string, object>
                {
                    ["Environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    ["MachineName"] = Environment.MachineName,
                    ["OSVersion"] = Environment.OSVersion.ToString(),
                    ["ProcessorCount"] = Environment.ProcessorCount,
                    ["UpTime"] = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()
                };

                // Check disk space
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
                foreach (var drive in drives)
                {
                    var freeSpacePercent = (double)drive.AvailableFreeSpace / drive.TotalSize * 100;
                    data[$"Drive_{drive.Name}_FreeSpace"] = $"{freeSpacePercent:F2}%";

                    if (freeSpacePercent < 10)
                        isHealthy = false;
                }

                // Check memory usage
                var process = Process.GetCurrentProcess();
                var memoryUsageMB = process.WorkingSet64 / (1024 * 1024);
                data["MemoryUsageMB"] = memoryUsageMB;

                if (!isHealthy)
                {
                    _logger.LogWarning("Health check failed");
                    return HealthCheckResult.Degraded("System resources are running low", data: data);
                }

                return HealthCheckResult.Healthy("All systems operational", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed with exception");
                return HealthCheckResult.Unhealthy("Health check failed", ex);
            }
        }
    }
}