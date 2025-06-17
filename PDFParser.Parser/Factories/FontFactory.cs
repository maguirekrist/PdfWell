using PDFParser.Parser.Document;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Factories;

public static class FontFactory
{
    public static Font Create(DictionaryObject fontDictionary, ObjectTable objectTable)
    {
        return new Font(fontDictionary, objectTable);
    }
}