namespace PDFParser.Parser.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class PdfDictionaryKeyAttribute : Attribute
{
    public string Key { get; }
    public PdfDictionaryKeyAttribute(string key)
    {
        Key = key;
    }
}