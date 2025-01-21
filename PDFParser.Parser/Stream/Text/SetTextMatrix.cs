using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;
using System.Diagnostics;

namespace PDFParser.Parser.Stream.Text;

internal class SetTextMatrix : ITextCommand
{

    internal SetTextMatrix(IReadOnlyList<DirectObject> objects)
    {

    }

    public void Execute(TextState textState)
    {
        throw new NotImplementedException();
    }
}
