using System.IO.Hashing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Statikk_Data.Helpers;

public static class SummonerUtilities
{
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
}