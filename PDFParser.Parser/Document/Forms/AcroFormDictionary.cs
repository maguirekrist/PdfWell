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
    private readonly DictionaryObject _dict;
    private readonly ObjectTable _objectTable;
    
    public AcroFormDictionary(DictionaryObject formDictionary, ObjectTable objectTable)
    {
        _dict = formDictionary;
        _objectTable = objectTable;
    }
    public List<AcroFormFieldDictionary> GetFields()
    {
        var fieldList = new List<AcroFormFieldDictionary>();

        foreach (var fieldReference in FieldReferences.Objects)
        {
            if (fieldReference is not ReferenceObject reference)
                throw new Exception("Expecting a reference and got something else.");
            
            var fieldObj = _objectTable.GetAs<DictionaryObject>(reference.Reference);
            
            //Ok, so you have a Field Object... what now? 
            //Fields are a complex polymorphic type almost in PDFs... 
            //Fields can be terminal only if the Kids array is null or if every object in the Kids array is explicitly not a FieldObject itself.
            //If the Field is terminal but only has 1 child widget... then that child Widget gets flattened, so the Field object will be both a
            //widget and a terminal field object. 
            
            
            var field = new AcroFormFieldDictionary(fieldObj);
            ExploreTree(field);
            fieldList.Add(field);
        }

        return fieldList.Flatten(n => n.Children).Where(x => x.IsTerminal).ToList();
    }

    private void ExploreTree(AcroFormFieldDictionary fieldObj)
    {
        if (fieldObj.IsTerminal || fieldObj.Kids == null) return;
        
        var kids = fieldObj.Kids;
        var kidObjects = new List<AcroFormFieldDictionary>();
        if (kids != null)
        {
            foreach (var kid in kids)
            {
                if (kid is ReferenceObject kidRef)
                {
                    var dict = _objectTable.GetAs<DictionaryObject>(kidRef.Reference);
                    var childField = new AcroFormFieldDictionary(dict);
                    ExploreTree(childField);
                    kidObjects.Add(childField);
                }
            }
            
        }
        
        //If kidObjects is not null
        fieldObj.Children.AddRange(kidObjects);
    }

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