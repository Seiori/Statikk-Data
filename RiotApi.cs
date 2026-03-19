using Statikk_Data.ENUMs;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace Statikk_Data;

public static class RiotApi
{
    private const string BaseUrl = "https://{0}.api.riotgames.com";

    public static readonly RegionalRoute[] RegionalRoutes = RegionalRouteExtensions.GetValues().ToArray();
    public static readonly PlatformRoute[] PlatformRoutes = PlatformRouteExtensions.GetValues().ToArray();
    public static readonly Tier[] Tiers = TierExtensions.GetValues().ToArray();
    public static readonly Division[] Divisions = DivisionExtensions.GetValues().ToArray();
    public static readonly Role[] Roles = RoleExtensions.GetValues().ToArray();

    private static readonly FrozenDictionary<RegionalRoute, string> RegionalUrls = RegionalRoutes
        .Where(regionalRoute => regionalRoute is not RegionalRoute.None)
        .ToFrozenDictionary(
            route => route, 
            route => string.Format(BaseUrl, route.GetStringLowerCase())
        );

    private static readonly FrozenDictionary<PlatformRoute, string> PlatformUrls = PlatformRoutes
        .Where(platformRoute => platformRoute is not PlatformRoute.None)
        .ToFrozenDictionary(
            route => route, 
            route => string.Format(BaseUrl, route.GetStringLowerCase())
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetBaseUrl(
        RegionalRoute regionalRoute
    )
    {
        return RegionalUrls[regionalRoute];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetBaseUrl(
        PlatformRoute platformRoute
    )
    {
        return PlatformUrls[platformRoute];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RegionalRoute GetRegionalRouteFromPlatformRoute(
        PlatformRoute platformRoute
    )
    {
        return platformRoute switch
        {
            PlatformRoute.Na1 or PlatformRoute.Br1 or PlatformRoute.La1 or PlatformRoute.La2
                => RegionalRoute.Americas,
            PlatformRoute.Kr or PlatformRoute.Jp1 
                => RegionalRoute.Asia,
            PlatformRoute.Eun1 or PlatformRoute.Euw1 or PlatformRoute.Me1 or PlatformRoute.Tr1 or PlatformRoute.Ru 
                => RegionalRoute.Europe,
            PlatformRoute.Oc1 or PlatformRoute.Sg2 or PlatformRoute.Tw2 or PlatformRoute.Vn2 
                => RegionalRoute.Sea,
            _ => throw new ArgumentOutOfRangeException(nameof(platformRoute), platformRoute, null)
        };
    }
}