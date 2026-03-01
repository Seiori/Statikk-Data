using Statikk_Data.ENUMs;
using System.Collections.Frozen;

namespace Statikk_Data;

public static class RiotApi
{
    private static readonly FrozenDictionary<PlatformRoute, RegionalRoute> RouteMappings = 
        new Dictionary<PlatformRoute, RegionalRoute>
        {
            { PlatformRoute.Na1, RegionalRoute.Americas },
            { PlatformRoute.Euw1, RegionalRoute.Europe },
            { PlatformRoute.Kr, RegionalRoute.Asia }
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<RegionalRoute, string> RegionalUrls = 
        new Dictionary<RegionalRoute, string>
        {
            { RegionalRoute.Americas, "https://americas.api.riotgames.com" },
            { RegionalRoute.Asia, "https://asia.api.riotgames.com" },
            { RegionalRoute.Europe, "https://europe.api.riotgames.com" }
        }.ToFrozenDictionary();

    private static readonly FrozenDictionary<PlatformRoute, string> PlatformUrls = 
        new Dictionary<PlatformRoute, string>
        {
            { PlatformRoute.Na1, "https://na1.api.riotgames.com" },
            { PlatformRoute.Euw1, "https://euw1.api.riotgames.com" },
            { PlatformRoute.Kr, "https://kr.api.riotgames.com" }
        }.ToFrozenDictionary();
    
    public static string GetUrl(RegionalRoute regionalRoute) => RegionalUrls[regionalRoute];

    public static string GetUrl(PlatformRoute platformRoute) => PlatformUrls[platformRoute];

    public static RegionalRoute GetRegionalRoute(PlatformRoute platform) => RouteMappings[platform];
}