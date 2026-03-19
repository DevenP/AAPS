using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Web.Services;

public class LogCleanupService : BackgroundService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ILogger<LogCleanupService> _logger;
    private readonly int _retentionDays;

    public LogCleanupService(
        IDbContextFactory<AppDbContext> dbFactory,
        ILogger<LogCleanupService> logger,
        IConfiguration configuration)
    {
        _dbFactory = dbFactory;
        _logger = logger;
        _retentionDays = configuration.GetValue<int>("Logging:DbRetentionDays", 90);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run once at startup (after a short delay so app is fully ready), then weekly
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var db = await _dbFactory.CreateDbContextAsync(stoppingToken);
                var cutoff = DateTime.UtcNow.AddDays(-_retentionDays);

                var deleted = await db.Database
                    .ExecuteSqlRawAsync(
                        "DELETE FROM Logs WHERE TimeStamp < {0}",
                        [cutoff],
                        stoppingToken);

                if (deleted > 0)
                    _logger.LogInformation("Log cleanup: deleted {Count} log entries older than {Days} days", deleted, _retentionDays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log cleanup failed");
            }

            await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
        }
    }
}
