namespace PDFParser.Parser.Objects;

public abstract class DirectObject
{
    public long Offset { get; }
    
    public long Length { get; }
    
    protected DirectObject(long offset, long length)
    {
        Offset = offset;
        Length = length;
    }
    
    public bool Equals(DirectObject? other)
    {
        if (other is null)
        {
            return false;
        }
        
        return other.Offset == Offset && other.Length == Length;
    }

    public override bool Equals(object? obj)
    {
        return obj is IndirectReference other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Offset, Length);
    }

    public override string ToString()
    {
        return $"offset: {Offset},  length: {Length}";
    }
    
}