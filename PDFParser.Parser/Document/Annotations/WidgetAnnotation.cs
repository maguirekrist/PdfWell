using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Document.Annotations;

public class WidgetAnnotation : AnnotationDictionary
{
    private readonly DictionaryObject _dict;
    public WidgetAnnotation(DictionaryObject annotationDict) : base(annotationDict)
    {
        if (annotationDict.GetAs<NameObject>("Subtype") is { Name: not "Widget" } typeObj)
        {
            throw new Exception($"Attempt to instantiate a {nameof(WidgetAnnotation)} object from a dictionary of type: {typeObj.Name}");
        }
        _dict = annotationDict;
    }

    public StringObject? HighlightMode => _dict.TryGetAs<StringObject>("H");
    public DirectObject? AppearanceCharacteristics => _dict.TryGetAs<DirectObject>("MK");
    public DirectObject? Action => _dict.TryGetAs<DirectObject>("A");
    public DirectObject? AdditionalActions => _dict.TryGetAs<DirectObject>("AA");
    public DirectObject? BorderStyle => _dict.TryGetAs<DirectObject>("BS");
    public DirectObject? Parent => _dict.TryGetAs<DirectObject>("Parent");
    public NumericObject? Rotation => _dict.TryGetAs<NumericObject>("R");
    public ArrayObject<DirectObject> ? BorderColor => _dict.TryGetAs<ArrayObject<DirectObject> >("BC");
    public ArrayObject<DirectObject> ? BackgroundColor => _dict.TryGetAs<ArrayObject<DirectObject> >("BG");
    
}