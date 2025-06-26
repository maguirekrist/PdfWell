using System.Collections.ObjectModel;
using PDFParser.Parser.ConceptObjects;
using PDFParser.Parser.Document;
using PDFParser.Parser.Document.Forms;
using PDFParser.Parser.Factories;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser;

public class PdfDocument
{
    private readonly Lazy<List<Page>> _pages;
    //private readonly Lazy<EncryptionDictionary?> _encryption;
    private readonly Lazy<DocumentCatalog> _catalog;
    
    private readonly ObjectTable _objectTable;
    private readonly Trailer? _trailer;
    
    public ObjectTable ObjectTable => _objectTable;
    
    public PdfDocument(ObjectTable objects, Trailer? trailer = null)
    {
        _objectTable = objects;
        _trailer = trailer;
        _pages = new Lazy<List<Page>>(LoadPages);
        //_encryption = new Lazy<EncryptionDictionary?>(GetEncryption);
        _catalog = new Lazy<DocumentCatalog>(GetDocumentCatalog);
    }
    
    public List<Page> Pages => _pages.Value;

    //public EncryptionDictionary? Encryption => _encryption.Value;
    public DocumentCatalog DocumentCatalog => _catalog.Value;

    public bool IsLinearized { get; init; } = false;

    public Page GetPage(int pageNumber)
    {
        if (pageNumber <= 0)
        {
            throw new ArgumentException("Pages are numbers 1 through N");
        }
        
        return Pages[pageNumber - 1];
    }

    private List<Page> LoadPages()
    {
        var pageObjects = _objectTable.Values.OfType<DictionaryObject>()
            .Where(x => x["Type"] is NameObject { Name: "Page" })
            .ToList();

        var pages = new List<Page>();
        foreach (var obj in pageObjects)
        {
            pages.Add(PageFactory.Create(obj, _objectTable));
        }

        return pages;
    }

    private DocumentCatalog GetDocumentCatalog()
    {
        var rootReference = _trailer.Root;
        var catalogDict = _objectTable.GetAs<DictionaryObject>(rootReference.Reference);
        return new DocumentCatalog(catalogDict);
    }

    //TODO: See if this is even needed at the document level.
    // private EncryptionDictionary? GetEncryption()
    // {
    //     // var encryptReference = _trailer.Encrypt;
    //     // if (encryptReference == null) return null;
    //     //
    //     // var encryptDict = _objectTable.GetAs<DictionaryObject>(encryptReference.Reference);
    //     // var dict = new EncryptionDictionary(encryptDict);
    //     //
    //     // dict.DecryptUser("");
    //     // return dict;
    //     return null;
    // }

    public AcroFormDictionary? GetAcroForm()
    {
        var acroRef = DocumentCatalog.AcroForm;
        if (acroRef == null) return null;

        var acroDictionary = _objectTable.GetAs<DictionaryObject>(acroRef.Reference);
        return new AcroFormDictionary(acroDictionary, _objectTable);
    }
}