using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Stream.Text;

public class SetTextDirection : ITextCommand
{

    private readonly NumericObject _moveTx;
    private readonly NumericObject _moveTy;
    
    public SetTextDirection(IReadOnlyList<DirectObject> objects)
    {
        _moveTx = objects[0] as NumericObject ?? throw new InvalidOperationException();
        _moveTy = objects[1] as NumericObject ?? throw new InvalidOperationException();
    }
    
    public void Execute(TextState textState)
    {
        //no-op
        //TODO: Implement Text Direction Command
        return;
    }
}