using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Stream.Text;

internal class SetFontFace : ITextCommand
{

    internal SetFontFace(IReadOnlyList<DirectObject> objects)
    {

    }

    public void Execute(TextState textState)
    {
        throw new NotImplementedException();
    }
}
