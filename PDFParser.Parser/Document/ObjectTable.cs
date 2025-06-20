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
}