using System.Collections.Concurrent;

namespace Statikk_Data;

public sealed class RiotApiRouteManager
{
    public readonly ConcurrentDictionary<string, RateLimiter> AppRateLimiters = new();
    public readonly ConcurrentDictionary<(string, string), RateLimiter> MethodRateLimiters = new();
}