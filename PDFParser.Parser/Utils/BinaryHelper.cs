using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PDFParser.Parser.Utils;

public class BinaryBuilder
{
    private List<byte[]> _buffers = new();
    public BinaryBuilder()
    {
    }

    public void Add(Span<byte> buffer)
    {
        _buffers.Add(buffer.ToArray());
    }

    public byte[] Build()
    {
        return BinaryHelper.Combine(_buffers.ToArray());
    }
}

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

    public static byte[] Combine(params byte[][] arrays)
    {
        var length = arrays.Sum(a => a.Length);
        var result = new byte[length];
        int offset = 0;

        foreach (var array in arrays)
        {
            Buffer.BlockCopy(array, 0, result, offset, array.Length);
            offset += array.Length;
        }

        return result;
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