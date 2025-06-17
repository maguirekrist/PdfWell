using PDFParser.Parser.Objects;

namespace PDFParser.Parser.ConceptObjects;

public class DocumentCatalog
{

    private readonly DictionaryObject _dict;
    
    public DocumentCatalog(DictionaryObject dict)
    {
        _dict = dict;
    }
    
    //Required - shall be /Catalog for the catalog dictionary
    public NameObject Type => _dict.GetAs<NameObject>("Type");

    //Optional. Used in incremental updates.
    public NameObject? Version => _dict.TryGetAs<NameObject>("Version");

    //optional - contains developer prefix identification and version numbers for developers. 
    public DictionaryObject? Extensions => _dict.TryGetAs<DictionaryObject>("Extensions");

    //Required - reference to the root of the Page Tree.
    public ReferenceObject Pages => _dict.GetAs<ReferenceObject>("Pages");
    
    //Optional - a name object specifying the page layout shall be used when document is opened.
    public NameObject? PageLayout => _dict.TryGetAs<NameObject>("PageLayout");

    //Optional - a name object specifying how the document shall be displayed when opened.
    public NameObject? PageMode => _dict.TryGetAs<NameObject>("PageMode");

    //Optional -> reference the documents interactive form dictionary
    public ReferenceObject? AcroForm => _dict.TryGetAs<ReferenceObject>("AcroForm");

    //Optional - a Metadata stream shall contain meatdata for the document. 
    public ReferenceObject? Metadata => _dict.TryGetAs<ReferenceObject>("Metadata");

    //Optional - the document-s structure tree root dictionary
    public DirectObject? StructTreeRoot => _dict.TryGetAs<DirectObject>("StructTreeRoot");
    
    //Optional - A mark information dictionary that shall contain information about the document's usage of tagged PDF conventions.
    public DictionaryObject? MarkInfo => _dict.TryGetAs<DictionaryObject>("MarkInfo");
}