
using System.Diagnostics;
using PDFParser.Parser.Document;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Stream.Text;

public class TextState
{
    public int[]? Matrix { get; set; }

    public Document.Font? Font { get; set; }

    public int? FontSize { get; set; }

    public string? Value { get; set; }

    
    public DocumentText GetText()
    {
        return new DocumentText(Value?.Trim('\0') ?? throw new UnreachableException(), FontSize ?? 0, Font?.Name ?? "Unknown", (0, 0));
    }
    
}
