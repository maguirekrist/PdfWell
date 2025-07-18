using PDFParser.Parser.Objects;

namespace PDFParser.Parser.ConceptObjects;

public class DocumentCatalog
{

    public DictionaryObject Dictionary { get; }
    
    public DocumentCatalog(DictionaryObject dict)
    {
        Dictionary = dict;
    }
    
    //Required - shall be /Catalog for the catalog dictionary
    public NameObject Type => Dictionary.GetAs<NameObject>("Type");

    //Optional. Used in incremental updates.
    public NameObject? Version => Dictionary.TryGetAs<NameObject>("Version");

    //optional - contains developer prefix identification and version numbers for developers. 
    public DictionaryObject? Extensions => Dictionary.TryGetAs<DictionaryObject>("Extensions");

    //Required - reference to the root of the Page Tree.
    public ReferenceObject Pages => Dictionary.GetAs<ReferenceObject>("Pages");
    
    //Optional - a name object specifying the page layout shall be used when document is opened.
    public NameObject? PageLayout => Dictionary.TryGetAs<NameObject>("PageLayout");

    //Optional - a name object specifying how the document shall be displayed when opened.
    public NameObject? PageMode => Dictionary.TryGetAs<NameObject>("PageMode");

    //Optional -> reference the documents interactive form dictionary
    public ReferenceObject? AcroForm => Dictionary.TryGetAs<ReferenceObject>("AcroForm");

    //Optional - a Metadata stream shall contain meatdata for the document. 
    public ReferenceObject? Metadata => Dictionary.TryGetAs<ReferenceObject>("Metadata");

    //Optional - the document-s structure tree root dictionary
    public DirectObject? StructTreeRoot => Dictionary.TryGetAs<DirectObject>("StructTreeRoot");
    
    //Optional - A mark information dictionary that shall contain information about the document's usage of tagged PDF conventions.
    public DictionaryObject? MarkInfo => Dictionary.TryGetAs<DictionaryObject>("MarkInfo");
}