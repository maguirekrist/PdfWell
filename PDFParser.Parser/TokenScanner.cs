namespace PDFParser.Parser;

public class TokenScanner
{

    private readonly ReadOnlyMemory<byte> _bytes;
    
    public int CurrentPosition { get; }
    
    public TokenScanner(
        ReadOnlyMemory<byte> bytes
        )
    {
        _bytes = bytes;
    }
    
    
    
}