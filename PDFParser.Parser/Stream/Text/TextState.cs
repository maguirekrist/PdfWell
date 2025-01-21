
using PDFParser.Parser.Document;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Stream.Text;

public class TextState
{
    public int[] Matrix { get; set; }

    public NameObject FontKey { get; set; }

    public int FontSize { get; set; }

    public string Value { get; set; }



    public DocumentText GetText()
    {
        return new DocumentText(Value, FontSize, FontKey.Name, (Matrix[0], Matrix[1]));
    }
    
}
