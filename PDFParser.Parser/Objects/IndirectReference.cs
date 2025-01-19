namespace PDFParser.Parser.Objects;

public readonly struct IndirectReference
{
    public int ObjectNumber { get; }
    public int Generation { get; }
    
    public IndirectReference(int objectNumber, int generation)
    {
        ObjectNumber = objectNumber;
        Generation = generation;
    }

    public override string ToString()
    {
        return $"{ObjectNumber} {Generation}";
    }
}