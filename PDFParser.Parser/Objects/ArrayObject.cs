namespace PDFParser.Parser.Objects;

public class ArrayObject : DirectObject
{
    public IReadOnlyList<DirectObject> Objects { get; }
    
    public ArrayObject(List<DirectObject> objects, long offset, long length) : base(offset, length)
    {
        Objects = objects;
    }

    public DirectObject this[int index] => Objects[index];

    public T GetAs<T>(int index) where T : DirectObject
    {
        if (index >= Objects.Count)
        {
            throw new IndexOutOfRangeException();
        }

        if (Objects[index] is T result)
        {
            return result;
        }

        throw new InvalidCastException($"The value at index '{index}' is not of type {typeof(T).Name}.");
    }
}