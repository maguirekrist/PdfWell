namespace PDFParser.Parser.Lexer;

public class TokenReader<T> where T : struct
{
    private readonly IReadOnlyList<T> _tokens;
    private int _index = 0;
    
    public TokenReader(IReadOnlyList<T> tokens)
    {
        _tokens = tokens;
    }

    public int Count => _tokens.Count;

    public T CurrentToken => _tokens[_index];

    public T Advance()
    {
        return _tokens[_index++];
    }

    public void Seek(int index)
    {
        _index = index;
    }

    public T? FirstOrDefault(Func<T, bool> predicate)
    {
        var token = _tokens.Select((item, index) => new { Item = item, Index = index })
            .FirstOrDefault(token => predicate(token.Item));
        if (token != null)
        {
            _index = token.Index;
            return token.Item;
        }
        
        return null;
    }
    
}