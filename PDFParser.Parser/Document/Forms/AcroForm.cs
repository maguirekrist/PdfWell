using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Document.Forms;

public class AcroForm
{
    private Lazy<List<AcroFormField>> _fields;
    private readonly DictionaryObject _formDictionary;
    
    public AcroForm(DictionaryObject formDictionary)
    {
        _formDictionary = formDictionary;
        _fields = new Lazy<List<AcroFormField>>(GetFields);
    }

    public List<AcroFormField> Fields => _fields.Value;

    private List<AcroFormField> GetFields()
    {
        return [];
    }
}