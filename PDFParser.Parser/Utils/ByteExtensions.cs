namespace PDFParser.Parser.Utils;


public static class ByteExtensions
{

    public static bool IsAlpha(this byte b)
    {
        return b is >= 65 and <= 90 or >= 97 and <= 122;
    }

    public static bool IsNumeric(this byte b)
    {
        return b is >= 48 and <= 57;
    }
}