namespace PDFParser.Parser.Objects;

public class ReferenceObject : DirectObject
{
    public IndirectReference Reference { get; }

    public DirectObject Value { get; }
    
    public ReferenceObject(IndirectReference reference, DirectObject value, long offset, long length) : base(offset, length)
    {
        Reference = reference;
        Value = value;
    }
}