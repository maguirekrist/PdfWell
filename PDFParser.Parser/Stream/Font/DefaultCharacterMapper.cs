using System.Text;

namespace PDFParser.Parser.Stream.Font;

public class DefaultCharacterMapper : IEncoding
{
    public DefaultCharacterMapper()
    {
    }

    public string GetString(ReadOnlySpan<byte> bytes)
    {
        return Encoding.ASCII.GetString(bytes);
    }
}