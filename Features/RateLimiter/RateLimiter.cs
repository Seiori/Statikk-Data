using System.Threading.RateLimiting;

namespace Statikk_Data.Features.RateLimiter;

public sealed class RateLimiter : IAsyncDisposable
{
    private volatile FixedWindowRateLimiter? _limiter;
    private readonly Lock _initLock = new();
    
    public RateLimiter(
        int limit, 
        TimeSpan window
    )
    {
        InitializeLimiter(limit, window);
    }
    
    public RateLimiter() { }

    public async ValueTask WaitAsync(
        CancellationToken cancellationToken
    )
    {
        var activeLimiter = _limiter;
        if (activeLimiter is not null)
        {
            var lease = await activeLimiter.AcquireAsync(1, cancellationToken).ConfigureAwait(false);
            if (!lease.IsAcquired)
            {
                throw new InvalidOperationException("Rate limit exceeded.");
            }
        }
    }

    public void TryInitialize(
        int limit, 
        TimeSpan window
    )
    {
        if (_limiter is not null)
        {
            return;
        }

        lock (_initLock)
        {
            if (_limiter is null)
            {
                InitializeLimiter(limit, window);
            }
        }
    }

    private void InitializeLimiter(
        int limit, 
        TimeSpan window
    )
    {
        _limiter = new FixedWindowRateLimiter(
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = limit,
                Window = window,
                AutoReplenishment = true,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10000 
            }
        );
    }

    public async ValueTask DisposeAsync()
    {
        if (_limiter is not null)
        {
            await _limiter.DisposeAsync().ConfigureAwait(false);
        }
    }
}