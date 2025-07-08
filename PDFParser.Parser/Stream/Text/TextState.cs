
using System.Diagnostics;
using System.Text;
using PDFParser.Parser.Document;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Stream.Text;

public class TextState
{
    private StringBuilder _stringBuilder = new();
    
    public int[]? Matrix { get; set; }

    public Document.Font? Font { get; set; }

    public int? FontSize { get; set; }

    public string? Text => _stringBuilder.ToString();
    
    public DocumentText GetText()
    {
        return new DocumentText(Text?.Trim('\0') ?? throw new UnreachableException(), FontSize ?? 0, Font?.Name ?? "Unknown", (0, 0));
    }

    public void AddText(string text)
    {
        _stringBuilder.Append(text);
        //_stringBuilder.Append(' ');
    }
    
}
