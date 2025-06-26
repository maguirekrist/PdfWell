using System.Diagnostics;
using System.IO.Compression;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Encryption;

public static class CompressionHandler
{

    public static Memory<byte> Decompress(StreamObject streamObject)
    {
        return Decompress(streamObject.Data, streamObject.Filter, streamObject.DecoderParams);
    }
    
    public static Memory<byte> Decompress(Span<byte> data, StreamFilter filter = StreamFilter.None, DecoderParams? decoderParams = null)
    {
        switch (filter)
        {
            case StreamFilter.None:
                return data.ToArray();
            case StreamFilter.Flate:
            {
                var rawBytes = HasZlibHeader(data) ? data.ToArray()[2..] : data.ToArray();
                using var memoryStream = new MemoryStream(rawBytes);
                using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
                using var output = new MemoryStream();
                deflateStream.CopyTo(output);

                var decodedBytes = output.ToArray().AsSpan();
                if (decoderParams.HasValue)
                {
                    decodedBytes = PngFilterDecompressor.Decompress(decodedBytes, decoderParams.Value);
                }
                return decodedBytes.ToArray();
            }
            default:
                throw new UnreachableException();
        }
    }
    
    private static bool HasZlibHeader(ReadOnlySpan<byte> data)
    {
        if (data.Length < 2)
            return false;

        byte cmf = data[0]; // Compression Method and Flags
        byte flg = data[1]; // Flags

        // CMF low nibble: compression method (should be 8 for deflate)
        if ((cmf & 0x0F) != 0x08)
            return false;

        // CMF high nibble: compression info (window size), should be <= 7 for spec
        if ((cmf >> 4) > 7)
            return false;

        // FLG bits 5-7: reserved, should be 0 in spec-compliant zlib
        if ((flg & 0b11100000) == 0b11100000)
            return false;

        // Validate header checksum: (CMF << 8 | FLG) % 31 == 0
        int combined = (cmf << 8) | flg;
        return combined % 31 == 0;
    }
    
}