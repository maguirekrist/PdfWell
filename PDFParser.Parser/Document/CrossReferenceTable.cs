using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Document;

public class CrossReferenceTable : Dictionary<IndirectReference, int>
{

    public void Extend(CrossReferenceTable other)
    {
        foreach (var kvp in other)
        {
            this[kvp.Key] = kvp.Value;
        }
    }
}