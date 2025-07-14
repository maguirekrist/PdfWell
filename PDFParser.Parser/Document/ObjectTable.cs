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

    public IndirectReference GetCatalogReference()
    {
        IndirectReference first = new IndirectReference();
        foreach (var x in this.Keys)
        {
            if (this[x] is DictionaryObject && (this[x] as DictionaryObject)?.Type?.Name == "Catalog")
            {
                first = x;
                break;
            }
        }

        return first;
    }
}