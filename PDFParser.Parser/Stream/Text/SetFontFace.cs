using PDFParser.Parser.Document;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Stream.Text;

public class SetFontFace : ITextCommand
{

    private readonly NameObject _fontKey;
    private readonly NumericObject _fontSize;
    private readonly Document.Font _font;
    
    public SetFontFace(IReadOnlyList<DirectObject> objects, Page page)
    {
        if (objects.Count != 2)
        {
            throw new ArgumentException();
        }
        
        if (objects[0] is not NameObject obj)
        {
            throw new ArgumentException($"Invalid object arguments, index 0 expected to be a NamedObject type. Received: ");
        }

        if (objects[1] is not NumericObject)
        {
            throw new ArgumentException("Test");
        }

        _fontKey = objects[0] as NameObject ?? throw new InvalidOperationException();
        _fontSize = objects[1] as NumericObject ?? throw new InvalidOperationException();
        _font = page.FontDictionary[_fontKey.Name];
    }

    public void Execute(TextState textState)
    {
        textState.Font = _font;
        textState.FontSize = (int)_fontSize.Value;
    }
}
