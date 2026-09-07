using AAPS.Application.Abstractions.Services;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AAPS.Infrastructure.Services;

/// <summary>
/// Background runner for the OverLapMandate flag recompute. Registered as a singleton so a single
/// coalesced worker serves the whole app: while a recompute is running, further requests just mark
/// that another pass is needed, so a burst of edits collapses into (at most) one more run.
/// </summary>
public sealed class FlagRecalculationService : IFlagRecalculationService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly ILogger<FlagRecalculationService> _logger;

    private readonly object _gate = new();
    private bool _running;
    private bool _pending;

    public FlagRecalculationService(IDbContextFactory<AppDbContext> factory, ILogger<FlagRecalculationService> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public bool IsRunning
    {
        get { lock (_gate) return _running; }
    }

    public event Action? StateChanged;

    public void Request()
    {
        bool started = false;
        lock (_gate)
        {
            _pending = true;
            if (!_running)
            {
                _running = true;
                started = true;
            }
        }

        // Only the request that flips us from idle to running kicks off the worker; others just
        // left _pending = true for the loop to pick up.
        if (started)
        {
            StateChanged?.Invoke();
            _ = RunLoopAsync();
        }
    }

    private async Task RunLoopAsync()
    {
        try
        {
            while (true)
            {
                lock (_gate)
                {
                    if (!_pending)
                    {
                        _running = false;
                        break;
                    }
                    _pending = false;
                }

                try
                {
                    await using var db = _factory.CreateDbContext();
                    db.Database.SetCommandTimeout(180);
                    await db.Database.ExecuteSqlRawAsync("EXEC OverLapMandate");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background flag recalculation failed");
                }
            }
        }
        finally
        {
            StateChanged?.Invoke();
        }
    }
}
