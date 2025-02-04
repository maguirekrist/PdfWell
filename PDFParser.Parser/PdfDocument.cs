using System.Collections.ObjectModel;
using PDFParser.Parser.Document;
using PDFParser.Parser.Factories;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser;

public class PdfDocument
{
    private readonly Lazy<List<Page>> _pages;
    
    private Dictionary<IndirectReference, DirectObject> _objectTable;
    
    public ReadOnlyDictionary<IndirectReference, DirectObject> ObjectTable => _objectTable.AsReadOnly();
    
    public PdfDocument(Dictionary<IndirectReference, DirectObject> objects)
    {
        _objectTable = objects;

        _pages = new Lazy<List<Page>>(LoadPages);
    }
    
    public List<Page> Pages => _pages.Value;

    public Page GetPage(int pageNumber)
    {
        if (pageNumber <= 0)
        {
            throw new ArgumentException("Pages are numbers 1 through N");
        }
        
        return Pages[pageNumber - 1];
    }

    public T GetObjectNumber<T>(int objNumber) where T : DirectObject
    {
        var obj = _objectTable.GetAs<T>(new IndirectReference(objNumber, 0));
        return obj;
    }

    private List<Page> LoadPages()
    {
        return _objectTable.Values.OfType<DictionaryObject>()
            .Where(x => x["Type"] is NameObject { Name: "Page" })
            .Select(x => PageFactory.Create(x, _objectTable))
            .ToList();
    }

}