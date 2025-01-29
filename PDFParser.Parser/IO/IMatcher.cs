namespace PDFParser.Parser.IO;

public interface IMatcher
{
     long? FindFirstOffset(ReadOnlyMemory<byte> stream, ReadOnlySpan<byte> pattern);
}