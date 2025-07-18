using System.Globalization;
using System.Text;
using PDFParser.Parser.Document.Annotations;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Document.Forms;

public enum FieldType
{
    Button,
    Text,
    Choice,
    Signature
}

[Flags]
public enum FieldFlags
{
    ReadOnly = 1 << 0,
    Required = 1 << 1,
    NoExport = 1 << 2
}

//Notes:
//A terminal field may have children that are widget annotations. They define the appearance on the page.
//As a convenience, when a field only has a single widget, the contents of the field dictionary and annotation
//can be merged into a single dictionary. 
//This means that this dictionary can potentially inherit all the values that belong to
//a AnnotationDictionary. 
public class AcroFormFieldDictionary
{
    private readonly DictionaryObject _dict;
    private readonly WidgetAnnotation? _widget;
    private readonly List<AcroFormFieldDictionary> _children;

    public AcroFormFieldDictionary(DictionaryObject dict, List<AcroFormFieldDictionary>? children = null)
    {
        _dict = dict;
        _children = children ?? new List<AcroFormFieldDictionary>();
        
        //Check if this dictionary is also a widget! 
        if (_dict.HasKey("Subtype") && _dict.TryGetAs<NameObject>("Subtype") is { Name: "Widget" })
        {
            _widget = new WidgetAnnotation(dict);
        }
    }

    public List<AcroFormFieldDictionary> Children => _children;

    public WidgetAnnotation? Widget => _widget;

    public Boolean IsTerminal => Type != null;
    
    public string GetValueAsString()
    {
        return FieldValue switch
        {
            StreamObject streamField => Encoding.UTF8.GetString(streamField.Data),
            NameObject nameField => nameField.Name,
            NumericObject numericField => numericField.Value.ToString(CultureInfo.InvariantCulture),
            StringObject stringField => stringField.Text,
            null => string.Empty,
            _ => string.Empty
        };
    }
    
    //Required for terminal fields. Inheritable.
    //Btn - Button Fields
    //Tx - text fields
    //Ch - choice Fields
    //Sig - signature fields
    public NameObject? Type => _dict.TryGetAs<NameObject>("FT");

    public DictionaryObject? Parent => _dict.TryGetAs<DictionaryObject>("Parent");
    public ArrayObject<DirectObject> ? Kids => _dict.TryGetAs<ArrayObject<DirectObject> >("Kids");

    //The partial field name
    public StringObject? FieldName => _dict.TryGetAs<StringObject>("T");

    public StringObject? AltDescription => _dict.TryGetAs<StringObject>("TU");

    //the name that shall be used when exporting interactive form field data from the document.
    public StringObject? MappingName => _dict.TryGetAs<StringObject>("TM");

    //A set of flags specifying various characteristcis of the field. Inheritable.
    private NumericObject? mFieldFlags => _dict.TryGetAs<NumericObject>("Ff");

    public FieldFlags? FieldFlags => mFieldFlags != null ? (FieldFlags)((int)mFieldFlags.Value) : null;
    
    //Optional, inheritable, the field's value, whose format varies depending on the field type. 
    public DirectObject? FieldValue => _dict.TryGetAs<DirectObject>("V");

    //Optional, inheritable, the default value to which the field reverts when a reset-form action is executed.
    //format shall be the same as V.
    public DirectObject? DefaultValue => _dict.TryGetAs<DirectObject>("DV");

    //Optional, an additional-actions dictionary defining the field's behaviour in response to various trigger events
    //
    public DictionaryObject? AdditionalActions => _dict.TryGetAs<DictionaryObject>("AA");
}