namespace PDFParser.Parser.Document;

public static class Operators
{
    //TEXT OPERATORS
    public static ReadOnlySpan<byte> SetTypeFace => "tf"u8;
    public static ReadOnlySpan<byte> SetTypeDirection => "td"u8;
    public static ReadOnlySpan<byte> SetTextMatrix => "tm"u8;
    public static ReadOnlySpan<byte> DisplayText => "TJ"u8;
    public static ReadOnlySpan<byte> BeginText => "BT"u8;
    public static ReadOnlySpan<byte> EndText => "ET"u8;

    // GRAPHICS STATE
    public static ReadOnlySpan<byte> PushState => "q"u8;
    public static ReadOnlySpan<byte> PopState => "Q"u8;

    public static ReadOnlySpan<byte> SetColor => "rg"u8;
    public static ReadOnlySpan<byte> SetStrokeColor => "RG"u8;

    public static ReadOnlySpan<byte> SetWidth => "w"u8;

    public static ReadOnlySpan<byte> DrawRectangle => "re"u8;
    public static ReadOnlySpan<byte> Fill => "F"u8;
    public static ReadOnlySpan<byte> Stroke => "S"u8;

}