using System.Diagnostics;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Stream.Font;

namespace PDFParser.Parser.Document;

public class Font
{
    private readonly DictionaryObject _fontDictionary;

    private readonly Lazy<IEncoding> _characterMapper;
    
    public Font(DictionaryObject fontDictionary)
    {
        if (fontDictionary.GetAs<NameObject>("Type").Name != "Font")
        {
            throw new ArgumentException("Font constructor requires a valid Font dictionary object.");
        }
        
        _fontDictionary = fontDictionary;
        _characterMapper = new Lazy<IEncoding>(CreateCharacterMapper);
    }

    public IEncoding CharacterMapper => _characterMapper.Value;

    public string Encoding => _fontDictionary.GetAs<NameObject>("Encoding").Name;

    public string SubType => _fontDictionary.GetAs<NameObject>("Subtype").Name;
    public string Name => _fontDictionary.GetAs<NameObject>("BaseFont").Name;
    
    public IEncoding CreateCharacterMapper()
    {
        if (_fontDictionary.HasKey("ToUnicode"))
        {
            //Construct a Unicode Character Mapper
            var test = _fontDictionary.GetAs<ReferenceObject>("ToUnicode");
            var cmapStream = test.Value as DictionaryObject ?? throw new UnreachableException();
            return new UnicodeCharacterMapper(cmapStream.Stream!.Reader);
        }   
        
        return new DefaultCharacterMapper();
    }
}