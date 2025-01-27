namespace PDFParser.Parser.Objects;

public class ReferenceObject : DirectObject
{
    private readonly Dictionary<IndirectReference, DirectObject> _objectTable;
    
    public IndirectReference Reference { get; }

    public DirectObject Value => _objectTable[Reference];
    
    public ReferenceObject(
        IndirectReference reference, 
        Dictionary<IndirectReference, DirectObject> objectTable, 
        long offset,
        long length) : base(offset, length)
    {
        Reference = reference;
        _objectTable = objectTable;
    }
    
    
}