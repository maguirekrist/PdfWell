using PDFParser.Parser.Math;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Document;

public class Text
{
    public Matrix<int> Transformation3X3 { get; }
    
    public NameObject FontKey { get; }
    
    public int FontSize { get;  }
    
    public string Value { get; }
    
    public Text(int[,] transform, NameObject fontKey, int fontSize, string text)
    {
        Transformation3X3 = new Matrix<int>(transform);
        FontKey = fontKey;
        FontSize = fontSize;
        Value = text;
    }
    
}