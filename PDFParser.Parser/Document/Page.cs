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
    internal void AddStream(StreamObject content)
    {
        _contents.Add(content);
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
                        case 'M':
                        case 'm':
                            //Text Matrix
                            commands.Add(new SetTextMatrix(tempList));
                            break;
                        case 'F':
                        case 'f':
                            //Font Face
                            commands.Add(new SetFontFace(tempList));
                            break;
                        case 'j':
                        case 'J':
                            //Text Display
                            commands.Add(new SetText(tempList));
                            break;
                        case 'D':
                        case 'd':
                            //Text Direction
                            commands.Add(new SetTextDirection(tempList));
                            break;
                        default:
                            throw new Exception($"Encountered an unexpected token in stream: {streamReader.CurrentChar}");
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

        var contents = pageDictionary["Contents"] ?? throw new UnreachableException();
        var streams = new List<StreamObject>();

        switch (contents)
        {
            case ReferenceObject contentReference:
            {
                AddStreamByReference(contentReference);
                break;
            }
            case ArrayObject contentArray:
            {
                foreach (var contentRef in contentArray.Objects.OfType<ReferenceObject>())
                {
                    AddStreamByReference(contentRef);
                }
                break;
            }
        }
        
        return new Page(mediaBox, streams);

        void AddStreamByReference(ReferenceObject reference)
        {
            var contentDict = objects.GetAs<DictionaryObject>(reference.Reference);
            var stream = contentDict.Stream;

            if (stream == null)
            {
                throw new UnreachableException();
            }

            streams.Add(stream);
        }
    }
    
}