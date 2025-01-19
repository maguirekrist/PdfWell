namespace PDFParser.Parser.Objects;

public class ArrayObject : DirectObject
{
    public IReadOnlyList<DirectObject> Objects { get; }
    
    public ArrayObject(List<DirectObject> objects, long offset, long length) : base(offset, length)
    {
        Objects = objects;
    }
}