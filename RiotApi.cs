using Statikk_Data.ENUMs;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

    public static string GetUrl(RegionalRoute route) => RegionalUrls[route];
    
    public static string GetUrl(PlatformRoute route) => PlatformUrls[route];

    public static RegionalRoute GetRegionalRoute(PlatformRoute platform) => RouteMappings[platform];
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long HashPuuid(ReadOnlySpan<char> puuid)
    {
        return (long)XxHash64.HashToUInt64(MemoryMarshal.AsBytes(puuid));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseGameId(string matchId, out long gameId)
    {
        var span = matchId.AsSpan();
        var underscoreIndex = span.LastIndexOf('_');
        if (underscoreIndex != -1)
        {
            return long.TryParse(span[(underscoreIndex + 1)..], out gameId);
        }
        gameId = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseMatchId(PlatformRoute platformRoute, long gameId, [NotNullWhen(true)] out string? matchId)
    {
        var prefix = platformRoute.GetStringUpperCase();
    
        Span<char> gameIdBuffer = stackalloc char[20]; 
        if (!gameId.TryFormat(gameIdBuffer, out var gameIdLength))
        {
            matchId = null;
            return false;
        }

        var totalLength = prefix.Length + 1 + gameIdLength;

        matchId = string.Create(totalLength, (prefix, gameId, gameIdLength), (span, state) =>
        {
            var (pName, gId, _) = state;
    
            pName.AsSpan().CopyTo(span);
    
            span[pName.Length] = '_';
    
            gId.TryFormat(span[(pName.Length + 1)..], out _);
        });

        return true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short GetPatchIdFromPatchVersion(string patchVersion)
    {
        if (string.IsNullOrEmpty(patchVersion)) return 0;

        var span = patchVersion.AsSpan();
        var len = span.Length;
        var major = 0;
        var minor = 0;
        var i = 0;

        while (i < len)
        {
            var c = span[i++];
            if (c == '.') break;
        
            var val = (uint)(c - '0');
            if (val > 9) return 0;
            major = (major * 10) + (int)val;
        }

        while (i < len)
        {
            var c = span[i++];
            if (c == '.') break;
        
            var val = (uint)(c - '0');
            if (val > 9) return 0;
            minor = (minor * 10) + (int)val;
        }

        return (short)(major * 100 + minor);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetPatchVersionFromPatchId(short patchId)
    {
        var major = patchId / 100;
        var minor = patchId % 100;

        var length = (major < 10 ? 1 : (major < 100 ? 2 : 3)) + 1 + (minor < 10 ? 1 : 2);

        return string.Create(length, (major, minor), (span, state) =>
        {
            state.major.TryFormat(span, out var charsWritten);
            span[charsWritten] = '.';
            state.minor.TryFormat(span[(charsWritten + 1)..], out _);
        });
    }
}