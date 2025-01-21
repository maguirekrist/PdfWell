using PDFParser.Parser.Stream.Text;

namespace PDFParser.Parser.Utils;

internal interface ITextCommand
{
    void Execute(TextState textState);
}
