namespace PDFParser.Parser.IO;

public interface IMatcher
{
     int? FindFirstOffset(ReadOnlyMemory<byte> stream, ReadOnlySpan<byte> pattern);
}