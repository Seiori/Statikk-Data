using System.Threading.RateLimiting;

namespace Statikk_Data;

public sealed class RateLimiter : IAsyncDisposable
{
    private const int DefaultLimit = 1;
    private static readonly TimeSpan DefaultWindow = TimeSpan.FromSeconds(1);

    private volatile FixedWindowRateLimiter _activeLimiter = CreateLimiter(DefaultLimit, DefaultWindow);

    private int _currentLimit = DefaultLimit;
    private TimeSpan _currentWindow = DefaultWindow;

    private readonly SemaphoreSlim _updateLock = new(1, 1);

    private static FixedWindowRateLimiter CreateLimiter(int limit, TimeSpan window)
    {
        return new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = limit,
            Window = window,
            AutoReplenishment = true,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 100
        });
    }

    public async ValueTask WaitAsync(CancellationToken cancellationToken)
    {
        using var lease = await _activeLimiter.AcquireAsync(1, cancellationToken);
        
        if (!lease.IsAcquired)
        {
            throw new InvalidOperationException("The rate limit queue is full. Reduce the number of concurrent tasks or increase the QueueLimit.");
        }
    }

    public async ValueTask SyncAsync(int newLimit, TimeSpan newWindow, int serverUsedCount)
    {
        if (_currentLimit == newLimit && _currentWindow == newWindow && serverUsedCount < newLimit * 0.9)
        {
            return;
        }

        FixedWindowRateLimiter? oldLimiter = null;
        await _updateLock.WaitAsync();

        try
        {
            if (_currentLimit != newLimit || _currentWindow != newWindow)
            {
                oldLimiter = _activeLimiter;
                var nextLimiter = CreateLimiter(newLimit, newWindow);

                if (serverUsedCount > 0)
                {
                    nextLimiter.AttemptAcquire(Math.Min(serverUsedCount, newLimit)).Dispose();
                }

                _activeLimiter = nextLimiter;
                _currentLimit = newLimit;
                _currentWindow = newWindow;
            }
        }
        finally
        {
            _updateLock.Release();
        }

        if (oldLimiter is not null)
        {
            await oldLimiter.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _activeLimiter.DisposeAsync();
        _updateLock.Dispose();
    }
}