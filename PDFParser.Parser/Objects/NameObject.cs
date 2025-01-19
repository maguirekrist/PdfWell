namespace PDFParser.Parser.Objects;

public class NameObject : DirectObject
{
    public string Name { get; }

    public NameObject(string name) : base(0, 0)
    {
        Name = name;
    }
    
    public NameObject(string name, long offset, long length) : base(offset, length)
    {
        Name = name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is NameObject other)
        {
            return string.Equals(Name, other.Name, StringComparison.Ordinal);
        }

        return false;
    }
    
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override string ToString()
    {
        return $"name: {Name}, {base.ToString()}";
    }
}