using System.Buffers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using PDFParser.Parser.Encryption;
using PDFParser.Parser.IO;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Stream.Text;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Document;

public class Page
{
    public PageBox MediaBox { get; }
    public int PageNumber { get; }
    
    private List<DocumentText> Texts => _texts.Value;
    
    private readonly ReadOnlyDictionary<IndirectReference, StreamObject> _contents;

    private readonly Dictionary<string, Font> _fontDictionary;

    private readonly Lazy<List<DocumentText>> _texts;

    private readonly EncryptionHandler? _encryptionHandler;
    
    public Page(PageBox mediaBox, ReadOnlyDictionary<IndirectReference, StreamObject> contents, Dictionary<string, Font> fontDictionary, int pageNumber, EncryptionHandler? encryptionHandler = null)
    {
        PageNumber = pageNumber;
        MediaBox = mediaBox;
        _contents = contents;
        _fontDictionary = fontDictionary;
        _encryptionHandler = encryptionHandler;
        _texts = new Lazy<List<DocumentText>>(GetTexts);
    }
    
    public ReadOnlyDictionary<IndirectReference, StreamObject> Contents => _contents;

    public ReadOnlyDictionary<string, Font> FontDictionary => _fontDictionary.AsReadOnly();

    public List<DocumentText> GetTexts()
    {
        var texts = new List<DocumentText>();

        foreach (var (key, content) in _contents)
        {
            var decryptedStream = content.Data;
            if (_encryptionHandler != null)
            {
                decryptedStream = _encryptionHandler.Decrypt(decryptedStream, key);
            }
            var stream = CompressionHandler.Decompress(decryptedStream, content.Filter, content.DecoderParams);
            var streamReader = new MemoryInputBytes(stream);
            var parser = new PdfParser(streamReader);
            while(!streamReader.IsAtEnd())
            {
                if (streamReader.ReadUntil(Operators.BeginText) == null)
                {
                    break;
                }
                
                //Operands come first, which are ALWAYS DirectObjects, we can effectively parse direct objects until 
                //a operation token is found
                 
                var commands = new List<ITextCommand>();
     
                while (!streamReader.IsAtEnd() && streamReader.CurrentChar != 'E')
                {
                    var tempList = new List<DirectObject>();
                    //var operandSet = new HashSet<char> { 'T', 'g', 'G', 'B', 'E', 'l', 'c', 'm', 'w', 'R', };
                    //TODO: Handle BDC and EMC (Begin/End Marked Content)
                    //TODO: Handle a lot of other graphical states... may need to rethink this whole function/process
                    while (!streamReader.IsAtEnd() && 
                           !streamReader.CurrentByte.IsAlpha())
                    {
                        
                        tempList.Add(parser.ParseDirectObject(streamReader));
                        streamReader.SkipWhitespace();
                    }

                    if (streamReader.CurrentChar == 'T')
                    {
                        streamReader.MoveNext();   
                    }
                    
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
                        case 'G':
                        case 'g':
                            //set Gray Level
                            //commands.Add(new SetGrayLevel((NumericObject)tempList[0], streamReader.CurrentChar == 'G'));
                            break;
                        case 'E':
                        case 'B':
                            //no-op
                            streamReader.Move(3);
                            break;
                        default:
                            //throw new Exception($"Encountered an unexpected token in stream: {streamReader.CurrentChar}");
                            //streamReader.MoveNext();
                            break;
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