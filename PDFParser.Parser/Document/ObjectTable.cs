using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Document;

public class ObjectTable : Dictionary<IndirectReference, DirectObject>
{
    public T GetObjectByNumber<T>(int objNumber) where T : DirectObject
    {
        var obj = this.GetAs<T>(new IndirectReference(objNumber, 0));
        return obj;
    }

    public IndirectReference? FirstKeyWhere(Predicate<DirectObject> predicate)
    {
        IndirectReference? first = null;
        foreach (var x in Keys)
        {
            if (predicate(this[x]))
            {
                first = x;
                break;
            }
        }

        return first;
    }

    public IReadOnlyList<IndirectReference> AllKeysWhere(Predicate<DirectObject> predicate)
    {
        List<IndirectReference> keys = new();

        foreach (var x in Keys)
        {
            if (predicate(this[x]))
            {
                keys.Add(x);
            }
        }
        
        return keys.AsReadOnly();
    }
    
    public IndirectReference GetCatalogReference()
    {
        return FirstKeyWhere(x => x is DictionaryObject { Type.Name: "Catalog" }) ?? throw new KeyNotFoundException();
    }
}