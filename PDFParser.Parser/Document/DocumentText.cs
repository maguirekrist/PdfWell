namespace PDFParser.Parser.Document;

public readonly struct DocumentText
{
    public (int, int) Position { get; }
    
    public string Value { get; }

    public int FontSize { get; }

    public string FontFamily { get; } 


    public DocumentText(string value, int fontSize, string fontFamily, (int, int) position)
    {
        Value = value;
        FontSize = fontSize;
        FontFamily = fontFamily;
        Position = position;
    }

    public override string ToString()
    {
        return $"text(font: {FontFamily}, size: {FontSize}): {Value}";
    }

}