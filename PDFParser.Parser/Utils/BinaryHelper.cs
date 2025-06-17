using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PDFParser.Parser.Utils;

public static class BinaryHelper
{
    public static int ReadInt32(ReadOnlySpan<byte> span)
    {
        return MemoryMarshal.Read<int>(span);
    }
    
    public static int ReadVariableIntBigEndian(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length > 4)
            throw new ArgumentOutOfRangeException(nameof(bytes), "Cannot read more than 4 bytes into an int.");

        var value = 0;
        foreach (var t in bytes)
        {
            value <<= 8;
            value |= t;
        }

        return value;
    }
    
    
    public static int ReadInt32BigEndian(ReadOnlySpan<byte> span)
    {
        return span.Length switch
        {
            2 => BinaryPrimitives.ReadInt16BigEndian(span),
            4 => BinaryPrimitives.ReadInt32BigEndian(span),
            _ => throw new UnreachableException()
        };
    }
}