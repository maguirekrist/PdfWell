using System.Diagnostics;

namespace PDFParser.Parser.Objects;

public class NullObject : DirectObject
{
    public NullObject(long offset, int length) : base(offset, length)
    {
        Debug.WriteLine("Null Object Created!");
    }
}