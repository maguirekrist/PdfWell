using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Document.Forms;

[Flags]
public enum SigFlags
{
    SignaturesExist = 1 << 1,
    AppendOnly = 1 << 2
}

public class AcroFormDictionary
{
    private Lazy<List<AcroFormFieldDictionary>> _fields;
    private readonly DictionaryObject _dict;
    private readonly ObjectTable _objectTable;
    
    public AcroFormDictionary(DictionaryObject formDictionary, ObjectTable objectTable)
    {
        _dict = formDictionary;
        _objectTable = objectTable;
        _fields = new Lazy<List<AcroFormFieldDictionary>>(GetFields);
    }
    private List<AcroFormFieldDictionary> GetFields()
    {
        var fieldList = new List<AcroFormFieldDictionary>();

        foreach (var fieldReference in FieldReferences.Objects)
        {
            if (fieldReference is not ReferenceObject reference)
                throw new Exception("Expecting a reference and got something else.");
            
            var fieldObj = _objectTable.GetAs<DictionaryObject>(reference.Reference);
            fieldList.Add(new AcroFormFieldDictionary(fieldObj));
        }

        return fieldList;
    }
    
    public List<AcroFormFieldDictionary> Fields => _fields.Value;

    public ArrayObject<DirectObject> FieldReferences => _dict.GetAs<ArrayObject<DirectObject>>("Fields");

    public BooleanObject? NeedAppearances => _dict.TryGetAs<BooleanObject>("NeedAppearances");

    //Optional - a set of flags specifying various doc-level characteristics related to signature fields.
    public NumericObject? SigFlags => _dict.TryGetAs<NumericObject>("SigFlags");

    public ArrayObject<DirectObject>? CalculationOrder => _dict.TryGetAs<ArrayObject<DirectObject>>("CO");

    public DictionaryObject? ResourceDictionary => _dict.TryGetAs<DictionaryObject>("DR");

    public StringObject? GlobalDefault_DA => _dict.TryGetAs<StringObject>("DA");

    public NumericObject? GlobalDefault_Q => _dict.TryGetAs<NumericObject>("Q");

    public DirectObject? XFA => _dict.TryGetAs<DirectObject>("XFA");
    
}