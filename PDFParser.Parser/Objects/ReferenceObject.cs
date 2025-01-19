namespace PDFParser.Parser.Objects;

public class ReferenceObject : DirectObject
{
    public IndirectReference Referece { get; }


    public ReferenceObject(IndirectReference reference, long offset, long length) : base(offset, length)
    {
        Referece = reference;
    }
}