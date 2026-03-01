using System.Runtime.CompilerServices;

namespace Statikk_Data.Helpers;

public static class Utilities
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string CreateCacheKey(string p1, string p2)
    {
        return string.Create(p1.Length + p2.Length, (p1, p2), (span, state) =>
        {
            state.p1.AsSpan().CopyTo(span);
            state.p2.AsSpan().CopyTo(span[state.p1.Length..]);
        });
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string CreateCacheKey(string p1, string p2, string p3)
    {
        return string.Create(p1.Length + p2.Length + p3.Length, (p1, p2, p3), (span, state) =>
        {
            var pos = 0;
            state.p1.AsSpan().CopyTo(span[pos..]); pos += state.p1.Length;
            state.p2.AsSpan().CopyTo(span[pos..]); pos += state.p2.Length;
            state.p3.AsSpan().CopyTo(span[pos..]);
        });
    }
}