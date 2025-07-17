using System.Collections;

namespace PDFParser.Parser.Objects;

public class ArrayObject<T> : DirectObject, IEnumerable<T> where T : DirectObject
{
    public IReadOnlyList<T> Objects { get; }

    public ArrayObject(List<T> objects) : base(0, 0)
    {
        Objects = objects;
    }
    
    public ArrayObject(List<T> objects, long offset, int length) : base(offset, length)
    {
        Objects = objects;
    }

    public DirectObject this[int index] => Objects[index];

    public int Count => Objects.Count;
    
    public TCast GetAs<TCast>(int index) where TCast : DirectObject
    {
        if (index >= Objects.Count)
        {
            throw new IndexOutOfRangeException();
        }

        if (Objects[index] is TCast result)
        {
            return result;
        }

        throw new InvalidCastException($"The value at index '{index}' is not of type {typeof(T).Name}.");
    }

    public IEnumerator<T> GetEnumerator() => Objects.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}