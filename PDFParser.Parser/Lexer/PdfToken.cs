namespace PDFParser.Parser.Lexer;

public readonly record struct PdfToken(TokenType Type, ReadOnlyMemory<byte> Lexeme, object? Literal = null)
{
}