using PDFParser.Parser.IO;

namespace PDFParser.Parser.Stream.Font;

public class UnicodeCharacterMapper : IEncoding
{
    //This may be inefficient 
    private Dictionary<ushort, char> _lookupTable;
    
    public UnicodeCharacterMapper()
    {
    }

    public string GetString(ReadOnlySpan<byte> bytes)
    {
        throw new NotImplementedException();
    }

    public static UnicodeCharacterMapper CreateUnicodeMapper(MemoryInputBytes cmapStream)
    {
        //TODO: Parse the CMap Stream
        throw new NotImplementedException();
    }
}