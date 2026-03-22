using System.Globalization;
using System.IO.Hashing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Statikk_Data.ENUMs;

namespace Statikk_Data.Helpers;

public static class Utilities
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ToPatchId(string patchVersion)
    {
        var parts = patchVersion.Split('.');
        if (parts.Length >= 2 &&
            short.TryParse(parts[0], out var major) &&
            short.TryParse(parts[1], out var minor)
        )
        {
            return (short)(major * 100 + minor);
        }
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToPatchVersion(short patchId)
    {
        var major = patchId / 100;
        var minor = patchId % 100;
        return $"{major}.{minor}";
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long HashPuuid(
        string puuid
    )
    {
        return (long)XxHash64.HashToUInt64(
            MemoryMarshal.AsBytes(
                puuid.AsSpan()
            )
        );
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseMatchId(
        string matchId, 
        out PlatformRoute platformRoute, 
        out long gameId
    )
    {
        platformRoute = default;
        gameId = 0;

        ReadOnlySpan<char> span = matchId;
        var sep = span.IndexOf('_');

        if (sep < 0)
        {
            return false;
        }

        if (!PlatformRouteExtensions.TryParse(span[..sep], out platformRoute))
        {
            return false;
        }

        return long.TryParse(
            span[(sep + 1)..],
            NumberStyles.None, 
            CultureInfo.InvariantCulture, 
            out gameId
        );
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetMatchId(
        PlatformRoute platform, 
        long gameId, 
        out string matchId
    )
    {
        matchId = string.Empty;
        ReadOnlySpan<char> platformSpan = platform.GetStringUpperCase();
    
        Span<char> buffer = stackalloc char[64];
        var position = 0;

        platformSpan.CopyTo(buffer);
        position += platformSpan.Length;

        buffer[position++] = '_';

        if (!gameId.TryFormat(buffer[position..], out var gameIdChars, default, CultureInfo.InvariantCulture))
        {
            return false;
        }

        position += gameIdChars;
        matchId = new string(buffer[..position]);
        return true;
    }
}