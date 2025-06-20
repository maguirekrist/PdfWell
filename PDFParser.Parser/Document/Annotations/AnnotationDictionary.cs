using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Document.Annotations;

[Flags]
public enum AnnotationFlags
{
    Invisible = 1 << 1,
    Hidden = 1 << 2,
    Print = 1 << 3,
    NoZoom = 1 << 4,
    NoRotate = 1 << 5,
    NoView = 1 << 6,
    ReadOnly = 1 << 7,
    Locked = 1 << 8,
    ToggleNoView = 1 << 9,
    LockedContents = 1 << 10
}

public class AnnotationDictionary
{
    private readonly DictionaryObject _dict;
    
    public AnnotationDictionary(DictionaryObject annotationDict)
    {
        _dict = annotationDict;
    }

    public AnnotationFlags? Flags => AnnotationFlags != null ? (AnnotationFlags)(int)AnnotationFlags.Value : null;
    
    public NameObject Subtype => _dict.GetAs<NameObject>("Subtype");
    public ArrayObject<DirectObject> Rect => _dict.GetAs<ArrayObject<DirectObject> >("Rect");
    public StringObject? Contents => _dict.TryGetAs<StringObject>("Contents");
    
    //Optional - an indirect reference to the page object with which this annotation is associated.
    public DictionaryObject? Page => _dict.TryGetAs<DictionaryObject>("P");

    public StringObject? AnnotationName => _dict.TryGetAs<StringObject>("NM");
    public DirectObject? LastModified => _dict.TryGetAs<StringObject>("M");
    public NumericObject? AnnotationFlags => _dict.TryGetAs<NumericObject>("F");
    //Can appear as an indirect reference.
    public DirectObject AppearanceDictionary => _dict.GetAs<DirectObject>("AP");

    //Optional - only required if the appearance dictionary AP contains one or more subdicts. 
    public NameObject? AppearanceState => _dict.TryGetAs<NameObject>("AS");

    public ArrayObject<DirectObject> ? Border => _dict.TryGetAs<ArrayObject<DirectObject> >("Border");

    public ArrayObject<DirectObject> ? Color => _dict.TryGetAs<ArrayObject<DirectObject> >("C");

    public NumericObject? StructParent => _dict.TryGetAs<NumericObject>("StructParent");
    public DirectObject? OptionalContentMembers => _dict.TryGetAs<DirectObject>("OC");
    public DirectObject? AssociatedFiles => _dict.TryGetAs<DirectObject>("AF");
    
    public NumericObject? Opacity => _dict.TryGetAs<NumericObject>("ca");
    public NumericObject? StrokeOpacity => _dict.TryGetAs<NumericObject>("CA");
    public StringObject? BlendMode => _dict.TryGetAs<StringObject>("BM");
    public StringObject? LanguageId => _dict.TryGetAs<StringObject>("Lang");
}