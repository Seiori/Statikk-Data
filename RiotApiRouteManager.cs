using System.Collections.Frozen;
using Statikk_Data.ENUMs;

namespace Statikk_Data;

public sealed class RiotApiRouteManager
{
    public readonly FrozenDictionary<RegionalRoute, RateLimiter> RegionalRateLimiters;
    public readonly FrozenDictionary<(RegionalRoute, Methods), RateLimiter> RegionalMethodRateLimiters;
    public readonly FrozenDictionary<PlatformRoute, RateLimiter> PlatformRateLimiters;
    public readonly FrozenDictionary<(PlatformRoute, Methods), RateLimiter> PlatformMethodRateLimiters;

    public RiotApiRouteManager()
    {
        var regionalRouteRateLimiters = new Dictionary<RegionalRoute, RateLimiter>(RegionalRouteExtensions.Length);
        var regionalRouteMethodRateLimiters = new Dictionary<(RegionalRoute, Methods), RateLimiter>(RegionalRouteExtensions.Length * MethodsExtensions.Length);

        foreach (var r in RegionalRouteExtensions.GetValues())
        {
            regionalRouteRateLimiters[r] = new RateLimiter();
            foreach (var m in MethodsExtensions.GetValues())
                regionalRouteMethodRateLimiters[(r, m)] = new RateLimiter();
        }

        RegionalRateLimiters = regionalRouteRateLimiters.ToFrozenDictionary();
        RegionalMethodRateLimiters = regionalRouteMethodRateLimiters.ToFrozenDictionary();

        var platformRouteRateLimiters = new Dictionary<PlatformRoute, RateLimiter>(PlatformRouteExtensions.Length);
        var platformRouteMethodRateLimiters = new Dictionary<(PlatformRoute, Methods), RateLimiter>(PlatformRouteExtensions.Length * MethodsExtensions.Length);

        foreach (var p in PlatformRouteExtensions.GetValues())
        {
            platformRouteRateLimiters[p] = new RateLimiter();
            foreach (var m in MethodsExtensions.GetValues())
                platformRouteMethodRateLimiters[(p, m)] = new RateLimiter();
        }

        PlatformRateLimiters = platformRouteRateLimiters.ToFrozenDictionary();
        PlatformMethodRateLimiters = platformRouteMethodRateLimiters.ToFrozenDictionary();
    }
}