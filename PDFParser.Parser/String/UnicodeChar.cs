using System.Globalization;
using System.Text;

namespace PDFParser.Parser.String;

public readonly record struct UnicodeChar(string Value)
{
    
    public UnicodeChar(byte[] utf16beBytes)
        : this(Encoding.BigEndianUnicode.GetString(utf16beBytes))
    { }

    public UnicodeChar(ReadOnlySpan<byte> utf16beBytes)
        : this(Encoding.BigEndianUnicode.GetString(utf16beBytes))
    { }

    public static UnicodeChar FromCodePoint(uint codePoint)
    {
        if (codePoint > 0x10FFFF)
            throw new ArgumentOutOfRangeException(nameof(codePoint));
        return new UnicodeChar(char.ConvertFromUtf32((int)codePoint));
    }

    public static UnicodeChar FromBigEndian(uint beCode)
    {
        // Reverse the bytes from big-endian
        uint codePoint =
            ((beCode & 0xFF000000) >> 24) |
            ((beCode & 0x00FF0000) >> 8)  |
            ((beCode & 0x0000FF00) << 8)  |
            ((beCode & 0x000000FF) << 24);

        // Now codePoint is a native-endian uint like 0x1F600

        if (codePoint > 0x10FFFF)
            throw new ArgumentOutOfRangeException(nameof(codePoint), "Code point must be ≤ U+10FFFF");

        return new UnicodeChar(char.ConvertFromUtf32((int)codePoint));
    }
    
    public int CodePointCount => new StringInfo(Value).LengthInTextElements;
}