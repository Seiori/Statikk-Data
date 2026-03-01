using System.Globalization;
using System.Runtime.CompilerServices;
using Statikk_Data.ENUMs;

namespace Statikk_Data.Helpers;

public static class MatchUtilities
{
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
    public static bool TryGetMatchId(PlatformRoute platform, long gameId, out string matchId)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParsePatchVersion(string patchVersion, out short patchId)
    {
        patchId = 0;
        ReadOnlySpan<char> span = patchVersion;

        var firstDot = span.IndexOf('.');
        if (firstDot == -1)
        {
            return false;
        }

        if (!short.TryParse(span[..firstDot], NumberStyles.None, CultureInfo.InvariantCulture, out var major))
        {
            return false;
        }

        var minorSpan = span[(firstDot + 1)..];

        var secondDot = minorSpan.IndexOf('.');
        if (secondDot != -1)
        {
            minorSpan = minorSpan[..secondDot];
        }

        if (!short.TryParse(minorSpan, NumberStyles.None, CultureInfo.InvariantCulture, out var minor))
        {
            return false;
        }

        patchId = (short)(major * 100 + minor);

        return true;
    }
}