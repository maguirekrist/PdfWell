namespace PDFParser.Parser.Objects;

public class BooleanObject : DirectObject
{
    
    public bool Value { get; }
    
    public BooleanObject(bool value, long offset, long length) : base(offset, length)
    {
        Value = value;
    }
}