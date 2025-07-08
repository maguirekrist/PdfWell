using System.Buffers;
using System.Text;
using PDFParser.Parser.IO;
using PDFParser.Parser.String;

namespace PDFParser.Parser.Objects;

public enum TextEncoding : byte
{
    Iso88591 = 0,
    Utf16 = 1,
    Utf16Be = 2,
    PdfDocEncoding = 3
}

public class StringObject : DirectObject
{
    //NOTE: string objects are very complex in PDF format
    //Strings need to be able to handle different encodings, including date information.
    //Encodings have specific rules that must be followed in order to parse them.
    //PDF String literals can escape special values with \ (backslash)
    //Strings are also regularly encoded as HEX streams that must be processed too!

    private readonly ReadOnlyMemory<byte> _data;
    
    //ublic TextEncoding TextEncoding { get; }
    
    public bool IsHex => _data.Span[0] == '<';

    public ReadOnlyMemory<byte> Data => _data;
    public byte[] Value { get; }

    public StringObject(ReadOnlyMemory<byte> data, long offset, long length) : base(offset, length)
    {
        _data = data;
        var codes = GetCharacterCodes();

        if (!IsHex)
        {
            var result = new List<byte>();
            for (var i = 0; i < codes.Length; i++)
            {
                if (codes[i] == '\\')
                {
                    i++;
                    if (i >= codes.Length) break;

                    switch ((char)codes[i])
                    {
                        case 'n': result.Add((byte)'\n'); break;
                        case 'r': result.Add((byte)'\r'); break;
                        case 't': result.Add((byte)'\t'); break;
                        case 'b': result.Add((byte)'\b'); break;
                        case 'f': result.Add((byte)'\f'); break;
                        case '(': result.Add((byte)'('); break;
                        case ')': result.Add((byte)')'); break;
                        case '\\': result.Add((byte)'\\'); break;
                        case '\n': break; // line continuation, skip
                        case '\r': break;
                        default:
                            // Octal parsing (up to 3 digits)
                            var octalDigits = $"{(char)codes[i]}";
                            for (var j = 0; j < 2 && i + 1 < codes.Length && codes[i + 1] >= '0' && codes[i + 1] <= '7'; j++)
                            {
                                i++;
                                octalDigits += (char)codes[i];
                            }
                            result.Add(Convert.ToByte(octalDigits, 8));
                            break;
                    }
                }
                else
                {
                    result.Add((byte)codes[i]);
                }
            }

            Value = result.ToArray();
        }
        else
        {
            Value = codes;
        }
        
    }

    // public byte[] GetBytes()
    // {
    //     switch (TextEncoding)
    //     {
    //         default:
    //             return IsoEncoding.StringAsBytes(Text);
    //     }
    // }

    private byte[] GetCharacterCodes()
    {
        var inner = _data.Span.Slice(1, _data.Length - 2);
        return IsHex ? 
            HexToBytes(inner) : 
            inner.ToArray();
    }

    private string DecodePdfString()
    {
        // UTF-16BE if BOM is present
        var bytes = _data.ToArray();
        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
        {
            return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
        }

        return Encoding.GetEncoding(28591).GetString(bytes);
    }

    private static byte[] HexToBytes(ReadOnlySpan<byte> data)
    {
        int len = data.Length;
        bool isOdd = (len % 2 != 0);
        int byteLen = (len + 1) / 2;

        byte[] bytes = new byte[byteLen];

        for (int i = 0; i < len; i += 2)
        {
            int hi = GetHexValue((char)data[i]);
            int lo;

            if (i + 1 < len)
                lo = GetHexValue((char)data[i + 1]);
            else
                lo = 0; // pad final nibble with 0 if odd-length

            bytes[i / 2] = (byte)((hi << 4) | lo);
        }

        return bytes;
    }
    private static int GetHexValue(char hex)
    {
        // Convert a single HEX character to its numeric value
        if (hex >= '0' && hex <= '9')
            return hex - '0';
        if (hex >= 'A' && hex <= 'F')
            return hex - 'A' + 10;
        if (hex >= 'a' && hex <= 'f')
            return hex - 'a' + 10;

        throw new ArgumentException($"Invalid HEX character: {hex}");
    }
}