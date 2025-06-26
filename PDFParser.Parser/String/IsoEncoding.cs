using System.Text;

namespace PDFParser.Parser.String;

public static class IsoEncoding
{
    private static readonly Encoding Iso88591 = Encoding.GetEncoding("ISO-8859-1");
    public static byte[] StringAsBytes(string s)
    {
        return Iso88591.GetBytes(s);
    }

    public static string BytesAsString(ReadOnlySpan<byte> bytes)
    {
        return Iso88591.GetString(bytes);
    }
}