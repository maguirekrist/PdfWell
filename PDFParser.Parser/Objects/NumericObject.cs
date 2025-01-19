namespace PDFParser.Parser.Objects;

public class NumericObject : DirectObject
{
    public double Value { get; }
    
    public NumericObject(double value, long offset, long length) : base(offset, length)
    {
        Value = value;
    }
}