using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;
using System.Diagnostics;
using PDFParser.Parser.Stream.Font;

namespace PDFParser.Parser.Stream.Text;

internal class SetText : ITextCommand
{

    //Text is either in two forms... character codes or ASCII.
    //Default PDF Fonts will encode text information in ascii, meaning, there is no Unicode lookup needed.
    //They just map the ascii code with the correlated glyph directly
    //In Unicode fonts, or custom fonts, there is a requirement to do a Unicode lookup.
    //All Font Resources SHOULD have a /ToUnicode reference, this is a pointer to a CMap Object (Stream Object).
    //Cmaps are a unique content stream type, similar to Content streams, where PDF objects are intermingled with operator tokens.
    
    
    private readonly byte[] _text;

    internal SetText(IReadOnlyList<DirectObject> objects)
    {
        var pre = objects[0];
        switch (pre)
        {
            case ArrayObject arrayObject:
                _text = arrayObject.Objects.OfType<StringObject>().Aggregate(new List<byte>(), (arr, x) =>
                {
                    arr.AddRange(x.Value);
                    return arr;
                }).ToArray();
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
        var currentFont = textState.Font;
        var mapper = currentFont?.CharacterMapper ?? new DefaultCharacterMapper();
        textState.Value = mapper.GetString(_text);
    }
}
