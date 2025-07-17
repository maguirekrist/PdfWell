using System.Diagnostics;
using System.Text;
using PDFParser.Parser.IO;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Lexer;

public class PdfTokenizer
{
    private readonly MemoryInputBytes _reader;
    private readonly List<PdfToken> _tokens = new();

    private int start;

    private static readonly HashSet<string> KeywordSet = new HashSet<string>()
    {
        "startxref",
        
    };

    public PdfTokenizer(MemoryInputBytes reader)
    {
        _reader = reader;
    }

    public IReadOnlyList<PdfToken> Tokens => _tokens.AsReadOnly();

    public IReadOnlyList<PdfToken> ScanTokens()
    {
        while (!_reader.IsAtEnd())
        {
            start = _reader.CurrentOffset;
            ScanToken();
            _reader.SkipWhitespace();
        }

        return Tokens;
    }

    private void ScanToken()
    {
        var current = _reader.Advance();
        switch (current)
        {
            case '<':
                if (_reader.CurrentChar == '<')
                {
                    _reader.MoveNext();
                    AddToken(TokenType.DictionaryBegin);
                }
                else
                {
                    ScanString(current);
                }
                break;
            case '>':
                if (_reader.CurrentChar == '>')
                {
                    _reader.MoveNext();
                    AddToken(TokenType.DictionaryEnd);
                }
                else
                {
                    throw new UnreachableException();
                }
                break;
            case '/':
                ScanName();
                break;
            case '%':
                ScanComment();
                break;
            case '(':
                ScanString(current);
                break;
            case '[':
                AddToken(TokenType.ArrayBegin);
                break;
            case ']':
                AddToken(TokenType.ArrayEnd);
                break;
            case var digit when char.IsDigit(digit):
            case '+':
            case '-':
            case '.':
                ScanNumeric(current);
                break;
            case '\n':
            case '\r':
            case ' ':
                _reader.MoveNext();
                break;
            case var alpha when char.IsAsciiLetter(alpha):
                //This is a upper or lowercase Ascii char.
                //Keyword match followed by some other shit? 
                var keyword = ScanKeyword(alpha);
                if (keyword == "stream")
                {
                    var endStreamBegin = _reader.ReadUntil("endstream"u8)!.Value;
                    _reader.Seek(endStreamBegin);
                    ScanKeyword();
                }
                break;
            default:
                throw new UnreachableException();
        }
    }

    private void AddToken(TokenType type)
    {
        var current = (int)_reader.CurrentOffset;
        _tokens.Add(new PdfToken(type, _reader.Slice(start, (current - start))));
    }

    private void AddToken(TokenType type, object literal)
    {
        var current = (int)_reader.CurrentOffset;
        _tokens.Add(new PdfToken(type, _reader.Slice(start, (current - start)), literal));
    }

    private string ScanKeyword(char? current = null)
    {
        var word = _reader.ReadAlpha();
        var wordStr = current + Encoding.ASCII.GetString(word);
        AddToken(TokenType.Keyword, wordStr);
        return wordStr;
    }

    private void ScanComment()
    {
        while (_reader.CurrentChar == '%')
        {
            _reader.MoveNext();
        }

        var commentBytes = _reader.ReadLine();
        var commentStr = Encoding.ASCII.GetString(commentBytes);
        AddToken(TokenType.Comment, commentStr);
    }
    
    private void ScanName()
    {
        var nameBytes = _reader.ReadName();

        if (nameBytes.Length == 0)
        {
            //TODO: Replace with custom ParseException
            throw new Exception();
        }

        var name = Encoding.ASCII.GetString(nameBytes);
        AddToken(TokenType.Name, name);
    }

    private void ScanString(char current)
    {
        switch (current)
        {
            case '(':
                do
                {
                    _reader.ReadUntil(")"u8);
                } while (_reader.LookBehind(2) is 92);
                break;
            case '<':
                _reader.ReadUntil(">"u8);
                break;
        }
        
        AddToken(TokenType.String, _reader.Slice(start, _reader.CurrentOffset - start));
    }
    
    private void ScanNumeric(char current)
    {
        var sign = 1;
        //var isFraction = false;
        switch (current)
        {
            case '-':
                sign = -1;
                _reader.MoveNext();
                break;
            case '+':
                sign = +1;
                _reader.MoveNext();
                break;
            case '.':
                //isFraction = true;
                _reader.MoveNext();
                break;
        }

        var number = _reader.ReadNumeric().ToArray();
        if (char.IsAsciiDigit(current))
        {
            var full = new byte[1 + number.Length];
            full[0] = (byte)current;
            number.CopyTo(full.AsMemory().Slice(1));
            number = full;
        }
        
        switch (_reader.CurrentChar)
        {
            case ' ':
                //It's we need to peek for a reference, which is a 3 byte look ahead.
                var refByte = _reader.LookAhead(3);
                if (refByte == 'R')
                {
                    //Ok we are parsing a reference
                    _reader.MoveNext();
                    var genNumber = _reader.ReadNumeric();
                    _reader.MoveNext();
                    _reader.MoveNext();

                    //var reference = new IndirectReference(int.Parse(number), int.Parse(genNumber));
                    // var offset = inputBytes.CurrentOffset;
                    // var directObject = ParseObjectByReference(reference);
                    // inputBytes.Seek(offset);
                    
                    //return new ReferenceObject(reference, begin, inputBytes.CurrentOffset - begin);
                    AddToken(TokenType.Reference, new IndirectReference(int.Parse(number), int.Parse(genNumber)));
                    return;
                }
                else
                {
                    //We are probably in an array just parsing a number, or it's just a random white space.
                    //return new NumericObject(sign * double.Parse(number), begin, inputBytes.CurrentOffset - begin, isFraction);
                    AddToken(TokenType.Numeric, sign * double.Parse(number));
                    return;
                }
            case '.':
                //still a number...
                
                _reader.MoveNext();
                var trailing = _reader.ReadNumeric();
                Span<byte> tempBuffer = stackalloc byte[number.Length + trailing.Length + 1];
                number.CopyTo(tempBuffer);
                tempBuffer[number.Length] = (byte)'.';
                trailing.CopyTo(tempBuffer[(number.Length + 1)..]);
                AddToken(TokenType.Numeric, sign * double.Parse(tempBuffer));
                return;
                //return new NumericObject(sign * double.Parse(tempBuffer), begin,  inputBytes.CurrentOffset - begin);
            default:
                //we are done parsing the number I think...
                //return new NumericObject(sign * double.Parse(number), begin, inputBytes.CurrentOffset - begin, isFraction);
                AddToken(TokenType.Numeric, sign * double.Parse(number));
                return;
        }
    }
}