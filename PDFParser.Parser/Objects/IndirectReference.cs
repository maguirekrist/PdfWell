namespace PDFParser.Parser.Objects;

public readonly struct IndirectReference : IEquatable<IndirectReference>
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

    public bool Equals(IndirectReference other)
    {
        return ObjectNumber == other.ObjectNumber && Generation == other.Generation;
    }

    public override bool Equals(object? obj)
    {
        return obj is IndirectReference other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ObjectNumber, Generation);
    }
    
    public static bool operator ==(IndirectReference left, IndirectReference right) => left.Equals(right);
    public static bool operator !=(IndirectReference left, IndirectReference right) => !left.Equals(right);
}