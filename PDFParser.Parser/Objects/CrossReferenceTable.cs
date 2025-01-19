namespace PDFParser.Parser.Objects;

public class CrossReferenceTable : DirectObject
{

    private readonly Dictionary<IndirectReference, long> _objectOffsets;

    public IReadOnlyDictionary<IndirectReference, long> ObjectOffsets => _objectOffsets;
    
    public CrossReferenceTable(
        IReadOnlyDictionary<IndirectReference, long> objectOffsets,
        long offset,
        long length
    ) : base(offset, length)
    {
        _objectOffsets = new Dictionary<IndirectReference, long>(objectOffsets);
    }
    
    
}