using System.Runtime.CompilerServices;

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
}