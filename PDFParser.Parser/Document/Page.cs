using System.Buffers;
using System.Collections.ObjectModel;
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

    private readonly Dictionary<string, Font> _fontDictionary;

    private readonly Lazy<List<DocumentText>> _texts;
    public Page(PageBox mediaBox, List<StreamObject> contents, Dictionary<string, Font> fontDictionary)
    {
        MediaBox = mediaBox;
        _contents = contents;
        _fontDictionary = fontDictionary;
        _texts = new Lazy<List<DocumentText>>(GetTexts);
    }
    
    public IReadOnlyList<StreamObject> Contents => _contents;

    public ReadOnlyDictionary<string, Font> FontDictionary => _fontDictionary.AsReadOnly();

    public List<DocumentText> GetTexts()
    {
        var texts = new List<DocumentText>();

        foreach (var content in _contents)
        {
            var streamReader = new MemoryInputBytes(content.Data);
            var parser = new PdfParser(streamReader);
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
                        
                        tempList.Add(parser.ParseDirectObject(streamReader));
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
                            commands.Add(new SetFontFace(tempList, this));
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
    
}