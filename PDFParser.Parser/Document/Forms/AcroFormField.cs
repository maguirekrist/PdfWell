namespace PDFParser.Parser.Document.Forms;

public enum FieldType
{
    Button,
    Text,
    Choice,
    Signature
}

public class AcroFormField
{
    public FieldType Type { get; }

    public AcroFormField(FieldType type)
    {
        Type = type;
    }
    
}