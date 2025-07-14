namespace PDFParser.Parser.Objects;

public class ReferenceObject : DirectObject
{
    //private readonly Dictionary<IndirectReference, DirectObject> _objectTable;
    
    public IndirectReference Reference { get; }

    //public DirectObject Value => _objectTable[Reference];
    
    public ReferenceObject(
        IndirectReference reference,
        long offset,
        int length) : base(offset, length)
    {
        Reference = reference;
    }

    public ReferenceObject(IndirectReference reference) : base(0, 0)
    {
        Reference = reference;
    }
}