using System.Collections.ObjectModel;
using PDFParser.Parser.ConceptObjects;
using PDFParser.Parser.Document;
using PDFParser.Parser.Document.Forms;
using PDFParser.Parser.Encryption;
using PDFParser.Parser.Factories;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser;

public class PdfDocument
{
    private readonly Lazy<List<Page>> _pages;
    private readonly Lazy<DocumentCatalog> _catalog;
    
    private readonly ObjectTable _objectTable;
    private readonly Trailer? _trailer;
    private readonly EncryptionHandler? _encryptionHandler;
    public ObjectTable ObjectTable => _objectTable;
    
    public PdfDocument(ObjectTable objects, Trailer? trailer = null, EncryptionHandler? encryptionHandler = null)
    {
        _objectTable = objects;
        _trailer = trailer;
        _encryptionHandler = encryptionHandler;
        _pages = new Lazy<List<Page>>(LoadPages);
        _catalog = new Lazy<DocumentCatalog>(GetDocumentCatalog);
    }
    
    public List<Page> Pages => _pages.Value;
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
            pages.Add(PageFactory.Create(obj, _objectTable, _encryptionHandler));
        }

        return pages;
    }

    private DocumentCatalog GetDocumentCatalog()
    {
        var rootReference = _trailer!.Root;
        var catalogDict = _objectTable.GetAs<DictionaryObject>(rootReference.Reference);
        return new DocumentCatalog(catalogDict);
    }

    public AcroFormDictionary? GetAcroForm()
    {
        var acroRef = DocumentCatalog.AcroForm;
        if (acroRef == null) return null;

        var acroDictionary = _objectTable.GetAs<DictionaryObject>(acroRef.Reference);
        return new AcroFormDictionary(acroDictionary, _objectTable);
    }
}