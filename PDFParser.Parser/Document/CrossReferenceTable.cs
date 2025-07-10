using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Document;

public class CrossReferenceTable : DirectObject
{

    private readonly Dictionary<IndirectReference, int> _objectOffsets;

    public IReadOnlyDictionary<IndirectReference, int> ObjectOffsets => _objectOffsets;
    
    public CrossReferenceTable(
        IReadOnlyDictionary<IndirectReference, int> objectOffsets,
        long offset,
        long length
    ) : base(offset, length)
    {
        _objectOffsets = new Dictionary<IndirectReference, int>(objectOffsets);
    }
    
    
}