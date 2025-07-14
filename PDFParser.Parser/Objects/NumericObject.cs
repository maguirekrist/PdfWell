namespace PDFParser.Parser.Objects;

public class NumericObject : DirectObject
{
    public double Value { get; }
    
    public NumericObject(double value, long offset, int length, bool? isFraction = false) : base(offset, length)
    {
        if (isFraction == true)
        {
            var numDigits = value.ToString().Length;
            var multiplier = System.Math.Pow(10, -numDigits);
            Value = value * multiplier;
        }
        else
        {
            Value = value;   
        }
    }
}