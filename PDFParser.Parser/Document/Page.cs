using System.Diagnostics;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Document;

public class Page
{
    public PageBox MediaBox { get; }

    public List<Text> Texts => _texts.Value;
    
    private readonly StreamObject _content;

    private readonly Lazy<List<Text>> _texts;
    public Page(PageBox mediaBox, StreamObject content)
    {
        MediaBox = mediaBox;
        _content = content;
        _texts = new Lazy<List<Text>>(GetTexts);
    }

    private List<Text> GetTexts()
    {
        throw new NotImplementedException();
    }
    
    //Factory Method that takes in a Dictionary
    public static Page Create(DictionaryObject pageDictionary, Dictionary<IndirectReference, DirectObject> objects)
    {
        var mediaBoxArr = pageDictionary.GetAs<ArrayObject>("MediaBox");
        var arguments = mediaBoxArr.Objects.OfType<NumericObject>().Select(x => (int)x.Value).ToArray();
        var mediaBox = new PageBox(arguments);

        var contents = pageDictionary.GetAs<ArrayObject>("Contents");
        var contentRef = contents.GetAs<ReferenceObject>(0);
        var contentDict = objects.GetAs<DictionaryObject>(contentRef.Reference);
        var stream = contentDict.Stream;

        if (stream == null)
        {
            throw new UnreachableException();
        }
        
        return new Page(mediaBox, stream);
    }
}