namespace PDFParser.Parser.Utils;

public static class SpanExtensions
{

    public static string ToAscii(this Span<byte> bytes)
    {
        return System.Text.Encoding.ASCII.GetString(bytes);
    }
    
    public static string ToAscii(this ReadOnlySpan<byte> bytes)
    {
        return System.Text.Encoding.ASCII.GetString(bytes);
    }
}