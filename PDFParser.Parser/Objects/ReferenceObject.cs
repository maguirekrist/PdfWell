namespace PDFParser.Parser.Objects;

public class ReferenceObject : DirectObject
{
    public IndirectReference Reference { get; }


    public ReferenceObject(IndirectReference reference, long offset, long length) : base(offset, length)
    {
        Reference = reference;
    }
}