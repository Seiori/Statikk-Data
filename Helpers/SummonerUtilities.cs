using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Statikk_Data.Helpers;

public static class SummonerUtilities
{
    private static readonly Guid Mosgi = Guid.Empty;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid ConvertPuuidToGuid(string puuid)
    {
        Span<byte> guidBytes = stackalloc byte[16];
        Mosgi.TryWriteBytes(guidBytes);

        BinaryPrimitives.ReverseEndianness(MemoryMarshal.Cast<byte, uint>(guidBytes[..4])[0]);
        BinaryPrimitives.ReverseEndianness(MemoryMarshal.Cast<byte, ushort>(guidBytes[4..6])[0]);
        BinaryPrimitives.ReverseEndianness(MemoryMarshal.Cast<byte, ushort>(guidBytes[6..8])[0]);

        var nameByteCount = Encoding.UTF8.GetByteCount(puuid);
        var nameBytes = nameByteCount <= 128 ? stackalloc byte[128] : new byte[nameByteCount];
        Encoding.UTF8.GetBytes(puuid, nameBytes);

        Span<byte> buffer = stackalloc byte[16 + nameByteCount];
        guidBytes.CopyTo(buffer);
        nameBytes[..nameByteCount].CopyTo(buffer[16..]);

        Span<byte> hash = stackalloc byte[20];
        SHA1.HashData(buffer, hash);

        hash[6] = (byte)((hash[6] & 0x0F) | 0x50);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);

        var result = hash[..16];
        BinaryPrimitives.ReverseEndianness(MemoryMarshal.Cast<byte, uint>(result[..4])[0]);
        BinaryPrimitives.ReverseEndianness(MemoryMarshal.Cast<byte, ushort>(result[4..6])[0]);
        BinaryPrimitives.ReverseEndianness(MemoryMarshal.Cast<byte, ushort>(result[6..8])[0]);

        return new Guid(result);
    }
}