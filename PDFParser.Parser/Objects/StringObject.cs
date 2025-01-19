namespace PDFParser.Parser.Objects;

public enum StringType
{
    Ascii,
    PdfDocEncoded,
    Text,
    Date
}

public class StringObject : DirectObject
{
    //NOTE: string objects are very complex in PDF format
    //Strings need to be able to handle different encodings, including date information.
    //Encodings have specific rules that must be followed in order to parse them.
    //PDF String literals can escape special values with \ (backslash)
    //Strings are also regularly encoded as HEX streams that must be processed too!

    private readonly ReadOnlyMemory<byte> _data;
    private readonly Lazy<StringType> _type;
    private readonly Lazy<string> _value;
    
    public bool IsHex => _data.Span[0] == '<';

    public StringType Type => _type.Value;

    public string Value => _value.Value;
        
    public StringObject(ReadOnlyMemory<byte> data, long offset, long length) : base(offset, length)
    {
        _data = data;
        _type = new Lazy<StringType>(GetStringType);
        _value = new Lazy<string>(GetString);
    }

    private StringType GetStringType()
    {
        return StringType.Ascii;
    }

    private string GetString()
    {
        if (IsHex)
        {
            //TODO: Parse Hex
        }
        else
        {
            
        }
        return String.Empty;
    }
}