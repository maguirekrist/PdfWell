namespace PDFParser.Parser.Stream.Font;

public interface IEncoding
{
    public string GetString(ReadOnlySpan<byte> bytes);
}