using System.Collections.ObjectModel;
using System.Diagnostics;
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
    private readonly Lazy<Dictionary<IndirectReference, Page>> _pageTable;
    private readonly ObjectTable _objectTable;
    private readonly EncryptionHandler? _encryptionHandler;
    
    public ObjectTable ObjectTable => _objectTable;
    
    public PdfDocument(ObjectTable objects, DocumentCatalog catalog, EncryptionHandler? encryptionHandler = null)
    {
        _objectTable = objects;
        _encryptionHandler = encryptionHandler;
        DocumentCatalog = catalog;
        _pageTable = new Lazy<Dictionary<IndirectReference, Page>>(LoadPages);
    }

    public List<Page> Pages => _pageTable.Value.Values.ToList();
    public DocumentCatalog DocumentCatalog { get; }
    public bool IsLinearized { get; init; } = false;

    public bool IsEncrypted => _encryptionHandler != null;

    public UserAccessPermissions? GetDocumentPermissions()
    {
        return _encryptionHandler?.EncryptionDictionary.DocumentPermissions;
    }
    
    public Page GetPage(int pageNumber)
    {
        if (pageNumber <= 0)
        {
            throw new ArgumentException("Pages are numbers 1 through N");
        }

        var page = Pages.FirstOrDefault(x => x.PageNumber == pageNumber) ?? throw new ArgumentException($"Page number {pageNumber} was not valid.");
        return page;
    }
    
    public Page GetPage(IndirectReference reference)
    {
        return _pageTable.Value[reference];
    }
    
    //private static void ExplorePageTree(DirectObject pageRoot, )
    
    private Dictionary<IndirectReference, Page> LoadPages()
    {
        var pageRootRef = DocumentCatalog.Pages;
        var store = new List<(int pageNumber, IndirectReference reference)>();
        
        var pageStack = new Stack<IndirectReference>();
        pageStack.Push(pageRootRef.Reference);
        var pageCounter = 1;
        
        while (pageStack.Any())
        {
            var topRef = pageStack.Pop();
            var obj = _objectTable[topRef];

            switch (obj)
            {
                case DictionaryObject { Type.Name: "Pages" } pagesDict:
                {
                    var kids = pagesDict.GetAs<ArrayObject<DirectObject>>("Kids");
                    foreach (var kid in kids.Reverse())
                    {
                        if (kid is ReferenceObject kidRef)
                        {
                            pageStack.Push(kidRef.Reference);
                        }
                    }
                    continue;
                }
                case DictionaryObject { Type.Name: "Page" } pageDict:
                {
                    store.Add((pageCounter, topRef));
                    pageCounter += 1;
                    continue;   
                }
                default:
                    throw new UnreachableException();
            }
        }
        
        var pageTable = new Dictionary<IndirectReference, Page>();
        foreach (var (pageNumber, pageRef) in store)
        {
            var pageObj = _objectTable.GetAs<DictionaryObject>(pageRef);
            pageTable.Add(pageRef, PageFactory.Create(pageObj, _objectTable, pageNumber, _encryptionHandler));
        }

        return pageTable;
    }

    public AcroFormDictionary? GetAcroForm()
    {
        var acroRef = DocumentCatalog.AcroForm;
        if (acroRef == null) return null;

        var acroDictionary = _objectTable.GetAs<DictionaryObject>(acroRef.Reference);
        return new AcroFormDictionary(acroDictionary, _objectTable);
    }

    public void Save(string path)
    {
        var acroForm = GetAcroForm();

        if (acroForm?.XFA != null)
        {
            //acroForm.Dictionary["NeedAppearances"] = new BooleanObject(true);
            
            if (acroForm.XFA is ArrayObject<DirectObject> xfaArr)
            {
                var xfaObjects = xfaArr.Objects.OfType<ReferenceObject>().ToList();
                foreach (var xfaObject in xfaObjects)
                {
                    _objectTable.Remove(xfaObject.Reference);
                }
            }
            
            //Essentially Remove all problematic things from AcroForms working properly. 
            
            acroForm.Dictionary.TryRemove("XFA");
            acroForm.Dictionary.TryRemove("SigFlags");

            //Names is weird, I need to do more research as to what this does.
            DocumentCatalog.Dictionary.TryRemove("Names");
            
            //Perms is very important, this Key-Value can restrict AcroForms from being editable in Acrobat... may be useful later. 
            DocumentCatalog.Dictionary.TryRemove("Perms");
        }
        
        using var writer = new PdfWriter(_objectTable, path);
        writer.Write();
    }
}