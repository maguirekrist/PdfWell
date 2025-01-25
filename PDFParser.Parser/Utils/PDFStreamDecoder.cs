using System.IO.Compression;

namespace PDFParser.Parser.Utils;

public static class PDFStreamDecoder
{

    public static byte[] DecodeFlate(byte[] compressedData)
    {
        using var memoryStream = new MemoryStream(compressedData);
        using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
        using var outputStream = new MemoryStream();
        
        deflateStream.CopyTo(outputStream);
        return outputStream.ToArray();
    }
}