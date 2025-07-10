namespace PDFParser.Parser.Lexer;

public enum TokenType
{
    //Syntax
    DictionaryBegin, DictionaryEnd,
    ArrayBegin, ArrayEnd,
    
    //Some Generic PDF primitives
    Numeric,
    Boolean,
    String,
    Name,
    Null,
    Comment,
    Reference,
    
    //Keywords
    Keyword //For now -- catch all Keyword type.
    
}