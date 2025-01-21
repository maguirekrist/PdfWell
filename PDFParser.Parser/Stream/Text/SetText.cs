using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;
using System.Diagnostics;

namespace PDFParser.Parser.Stream.Text;

internal class SetText : ITextCommand
{

    private readonly string _text;

    internal SetText(IReadOnlyList<DirectObject> objects)
    {
        var pre = objects[0];
        switch (pre)
        {
            case ArrayObject arrayObject:
                _text = string.Join("", arrayObject.Objects.OfType<StringObject>().Select(x => x.Value));
                break;
            case StringObject stringObject:
                _text = stringObject.Value;
                break;
            default:
                throw new UnreachableException();
        }
    }

    public void Execute(TextState textState)
    {
        textState.Value = _text;
    }
}
