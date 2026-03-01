using System.Threading.RateLimiting;

namespace Statikk_Data;

public sealed class RateLimiter : IAsyncDisposable
{
    private const int DefaultLimit = 10;
    private static readonly TimeSpan DefaultWindow = TimeSpan.FromSeconds(1);

    private volatile FixedWindowRateLimiter _activeLimiter;
    private int _currentLimit;
    private TimeSpan _currentWindow;

    private readonly SemaphoreSlim _updateLock = new(1, 1);

    public RateLimiter()
    {
        _currentLimit = DefaultLimit;
        _currentWindow = DefaultWindow;
        _activeLimiter = CreateLimiter(_currentLimit, _currentWindow);
    }

    private static FixedWindowRateLimiter CreateLimiter(int limit, TimeSpan window)
    {
        return new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = limit,
            Window = window,
            AutoReplenishment = true,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 1000 
        });
    }

    public async ValueTask WaitAsync(CancellationToken cancellationToken)
    {
        using var lease = await _activeLimiter.AcquireAsync(1, cancellationToken);
        
        if (!lease.IsAcquired)
        {
            throw new InvalidOperationException();
        }
    }

    public async ValueTask SyncAsync(int newLimit, TimeSpan newWindow, int serverUsedCount)
    {
        if (_currentLimit == newLimit && _currentWindow == newWindow)
        {
            return;
        }

        await _updateLock.WaitAsync();

        try
        {
            if (_currentLimit != newLimit || _currentWindow != newWindow)
            {
                var oldLimiter = _activeLimiter;
                var nextLimiter = CreateLimiter(newLimit, newWindow);

                if (serverUsedCount > 0)
                {
                    nextLimiter.AttemptAcquire(Math.Min(serverUsedCount, newLimit)).Dispose();
                }

                _activeLimiter = nextLimiter;
                _currentLimit = newLimit;
                _currentWindow = newWindow;

                await oldLimiter.DisposeAsync();
            }
        }
        finally
        {
            _updateLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _activeLimiter.DisposeAsync();
        _updateLock.Dispose();
    }
}