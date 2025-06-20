using System.Buffers;
using System.Text;

namespace PDFParser.Parser.Objects;

public class StringObject : DirectObject
{
    //NOTE: string objects are very complex in PDF format
    //Strings need to be able to handle different encodings, including date information.
    //Encodings have specific rules that must be followed in order to parse them.
    //PDF String literals can escape special values with \ (backslash)
    //Strings are also regularly encoded as HEX streams that must be processed too!

    private readonly ReadOnlyMemory<byte> _data;
    private readonly Lazy<byte[]> _value;
    private readonly Lazy<string> _text;
    
    public bool IsHex => _data.Span[0] == '<';
    public byte[] Value => _value.Value;
    public int Length => _data.Length;
    public int ByteLength => _value.Value.Length;

    public string Text => _text.Value;
        
    public StringObject(ReadOnlyMemory<byte> data, long offset, long length) : base(offset, length)
    {
        _data = data;
        _value = new Lazy<byte[]>(GetCharacterCodes);
        _text = new Lazy<string>(DecodePdfString);
    }

    private byte[] GetCharacterCodes()
    {
        return IsHex ? 
            HexToBytes(_data.Span.Slice(1, _data.Length - 2)) : 
            _data.Span.Slice(1, _data.Length - 1).ToArray();
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

    private byte[] HexToBytes(ReadOnlySpan<byte> data)
    {
        if (data.Length % 2 != 0)
        {
            throw new ArgumentException("Invalid Hex stream!");
        }

        var bytes = new byte[data.Length / 2];
        //byte[] bytes = new byte[data.Length / 2];
        for (int i = 0; i < data.Length; i += 2)
        {
            int highNibble = GetHexValue((char)data[i]) << 4;
            int lowNibble = GetHexValue((char)data[i + 1]);
            bytes[i / 2] = (byte)(highNibble | lowNibble);
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