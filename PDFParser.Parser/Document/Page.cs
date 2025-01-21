using System.Buffers;
using System.Diagnostics;
using PDFParser.Parser.IO;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Stream.Text;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Document;

public class Page
{
    public PageBox MediaBox { get; }

    public List<DocumentText> Texts => _texts.Value;
    
    private readonly List<StreamObject> _contents;

    private readonly Lazy<List<DocumentText>> _texts;
    public Page(PageBox mediaBox, List<StreamObject> contents)
    {
        MediaBox = mediaBox;
        _contents = contents;
        _texts = new Lazy<List<DocumentText>>(GetTexts);
    }

    public List<DocumentText> GetTexts()
    {
        var texts = new List<DocumentText>();

        foreach (var content in _contents)
        {
         var streamReader = content.GetReader();
             while(!streamReader.IsAtEnd())
             {
                 try
                 {
                     streamReader.ReadUntil(Operators.BeginText);
                 }
                 catch (Exception ex)
                 {
                     break;
                 }
                 //Operands come first, which are ALWAYS DirectObjects, we can effectively parse direct objects until 
                 //a operation token is found
                 if(streamReader.IsAtEnd())
                 {
                     break;
                 }
                 
                 var commands = new List<ITextCommand>();
     
                 while (streamReader.CurrentChar != 'E')
                 {
                     var tempList = new List<DirectObject>();
     
                     while (!streamReader.IsAtEnd() && streamReader.CurrentByte != 'T')
                     {
                         tempList.Add(PdfParser.ParseDirectObject(streamReader));
                         streamReader.SkipWhitespace();
                     }
     
                     streamReader.MoveNext();
                     switch (streamReader.CurrentChar)
                     {
                         case 'm':
                             //Text Matrix
                             commands.Add(new SetTextMatrix(tempList));
                             break;
                         case 'f':
                             //Font Face
                             commands.Add(new SetFontFace(tempList));
                             break;
                         case 'J':
                             //Text Display
                             commands.Add(new SetText(tempList));
     
                             break;
                         default:
                             throw new UnreachableException();
                     }
     
                     streamReader.MoveNext();
                     streamReader.SkipWhitespace();
                 }
     
                 TextState textState = new();
                 foreach (var command in commands)
                 {
                     command.Execute(textState);
                 }
     
                 texts.Add(textState.GetText());
             }   
        }

        return texts;
    }


    
    //Factory Method that takes in a Dictionary
    public static Page Create(DictionaryObject pageDictionary, Dictionary<IndirectReference, DirectObject> objects)
    {
        var mediaBoxArr = pageDictionary.GetAs<ArrayObject>("MediaBox");
        var arguments = mediaBoxArr.Objects.OfType<NumericObject>().Select(x => (int)x.Value).ToArray();
        var mediaBox = new PageBox(arguments);

        var contents = pageDictionary.GetAs<ArrayObject>("Contents");

        var streams = new List<StreamObject>();

        foreach (var contentRef in contents.Objects.OfType<ReferenceObject>())
        {
            var contentDict = objects.GetAs<DictionaryObject>(contentRef.Reference);
            var stream = contentDict.Stream;
            
            if (stream == null)
            {
                throw new UnreachableException();
            }

            streams.Add(stream);
        }
        
        return new Page(mediaBox, streams);
    }
}