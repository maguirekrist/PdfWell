namespace PDFParser.Parser.Objects;

public readonly struct IndirectReference
{
    public int ObjectNumber { get; }
    public int Generation { get; }
    
    public IndirectReference(int objectNumber, int generation = 0)
    {
        ObjectNumber = objectNumber;
        Generation = generation;
    }

    public override string ToString()
    {
        return $"obj: {ObjectNumber} {Generation}";
    }
}