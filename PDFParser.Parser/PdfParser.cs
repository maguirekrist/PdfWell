using System.Diagnostics;
using System.Text;
using PDFParser.Parser.IO;
using PDFParser.Parser.Objects;
namespace PDFParser.Parser;

public class PdfParser
{
    private readonly MemoryInputBytes _memoryReader;

    public PdfParser(byte[] pdfData)
    {
        _memoryReader = new MemoryInputBytes(pdfData);
    }
    
    public PdfDocument Parse()
    {
        var startXref = FindStartXrefOffset(_memoryReader);
        _memoryReader.Seek(0);

        var xrefTable = ParseXrefTable(_memoryReader, startXref);

        var objectTable = new Dictionary<IndirectReference, DirectObject>();
        
        foreach (var (key, val) in xrefTable.ObjectOffsets)
        {
            objectTable.Add(key, ParseObjectByReference(_memoryReader, key, xrefTable));
        }

        return new PdfDocument(objectTable);
    }

    private static string FindPdfVersion(MemoryInputBytes inputBytes)
    {
        //Get the first line
        throw new NotImplementedException();
    }

    private static long FindStartTrailer(MemoryInputBytes inputBytes)
    {
        ReadOnlySpan<byte> trailerMarker = "trailer"u8;
        return inputBytes.FindFirstPatternOffset(trailerMarker);
    }

    private static long FindStartXrefOffset(MemoryInputBytes inputBytes)
    {
        ReadOnlySpan<byte> startXrefMarker = "startxref"u8;
        var startXref = inputBytes.FindFirstPatternOffset(startXrefMarker);
        inputBytes.MoveNext();
        var nextLine = inputBytes.ReadLine();
        long.TryParse(nextLine, out var result);
        return result;
    }

    private static CrossReferenceTable ParseXrefTable(MemoryInputBytes inputBytes, long startXref)
    {
        inputBytes.Seek(startXref);
        var xrefTable = new Dictionary<IndirectReference, long>();

        ReadOnlySpan<byte> xrefMarker = "xref"u8;
        if (!inputBytes.Match(xrefMarker))
        {
            throw new Exception("Expected xref in next sequence of bytes.");
        }

        inputBytes.MoveNext();
        
        var line = inputBytes.ReadLine();
        int.TryParse(line.Slice(1), out var objectCount);

        for (var i = 0; i < objectCount; i++)
        {
            line = inputBytes.ReadLine();
            var objectOffset = long.Parse(line.Slice(0, 10));
            if (objectOffset != 0)
            {
                xrefTable.Add(new IndirectReference(i, 0), objectOffset);
            }
        }
        
        return new CrossReferenceTable(xrefTable, startXref, inputBytes.CurrentOffset - startXref);
    }

    private static DirectObject ParseObjectByReference(
        MemoryInputBytes inputBytes,
        IndirectReference objectReference,
        CrossReferenceTable table)
    {
        // 1st get Byte Offset from Cross Reference Table
        var offset = table.ObjectOffsets[objectReference];
        inputBytes.Seek(offset);
        inputBytes.ReadLine();

        try
        {
            var dObject = ParseDirectObject(inputBytes);
            if (dObject is DictionaryObject dictionaryObject && dictionaryObject.IsStream)
            {
                //TODO: Read the stream
                var lengthObject = dictionaryObject["Length"];
                if (lengthObject != null && lengthObject is NumericObject lengthNumeric)
                {
                    var streamLength = (int)lengthNumeric.Value;
                    dictionaryObject.Stream = ParseStreamObject(inputBytes, (int)lengthNumeric.Value);
                    inputBytes.Seek(inputBytes.CurrentOffset + streamLength);
                    inputBytes.MoveNext();
                    var endStreamLine = inputBytes.ReadLine();
                    Debug.Assert(endStreamLine.SequenceEqual("endstream"u8));
                }
                else
                {
                    throw new UnreachableException();
                }
            }
        
            return dObject;
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception was thrown trying to parse object: {objectReference}", ex);
        }
    }

    private static StreamObject ParseStreamObject(MemoryInputBytes inputBytes, int length)
    {
        //Move to the next line
        inputBytes.MoveNext();
        
        var streamBegin = inputBytes.ReadLine();
        if (!streamBegin.SequenceEqual("stream"u8))
        {
            throw new UnreachableException();
        }

        //Beginning of stream
        var begin = inputBytes.CurrentOffset;
        return new StreamObject(inputBytes.Slice((int)begin, length), begin, length);
    }

    private static DictionaryObject ParseDictionaryObject(MemoryInputBytes inputBytes)
    {
        //We are outside of the dictionary
        var beginOffset = inputBytes.CurrentOffset;
        var dictionary = new Dictionary<NameObject, DirectObject>();

        if (!inputBytes.Match("<<"u8))
        {
            throw new Exception("What the ehck?!!?");
        }
        
        while (!inputBytes.IsAtEnd())
        {
            switch (inputBytes.CurrentChar)
            {
                case '/':
                    var nameObject = ParseNameObject(inputBytes);
                    var valueObject = ParseDirectObject(inputBytes);
                    dictionary.Add(nameObject, valueObject);
                    break;
                case '>':
                    if (inputBytes.Peek() == '>')
                    {
                        //Close out the dictinonary!
                        inputBytes.MoveNext();
                        inputBytes.MoveNext();
                        return new DictionaryObject(dictionary, beginOffset, inputBytes.CurrentOffset - beginOffset);
                    }
                    break;
                default:
                    inputBytes.MoveNext();
                    break;
            }
        }
        
        throw new Exception("Failed parsing dictionary.");
    }

    private static DirectObject ParseNumericObject(MemoryInputBytes inputBytes)
    {
        var begin = inputBytes.CurrentOffset;
        var sign = 1;
        var isFraction = false;
        
        switch (inputBytes.CurrentChar)
        {
            case '-':
                sign = -1;
                inputBytes.MoveNext();
                break;
            case '+':
                sign = +1;
                inputBytes.MoveNext();
                break;
            case '.':
                isFraction = true;
                inputBytes.MoveNext();
                break;
        }
        
        var number = inputBytes.ReadNumeric();
        switch (inputBytes.CurrentChar)
        {
            case ' ':
                //It's we need to peek for a reference, which is a 3 byte look ahead.
                var refByte = inputBytes.LookAhead(3);
                if (refByte == 'R')
                {
                    //Ok we are parsing a reference
                    inputBytes.MoveNext();
                    var genNumber = inputBytes.ReadNumeric();
                    inputBytes.MoveNext();
                    inputBytes.MoveNext();
                
                    return new ReferenceObject(new IndirectReference(int.Parse(number), int.Parse(genNumber)),
                        begin, inputBytes.CurrentOffset - begin);
                }
                else
                {
                    //We are probably in an array just parsing a number, or it's just a random white space.
                    return new NumericObject(sign * double.Parse(number), begin, inputBytes.CurrentOffset - begin, isFraction);
                }
            case '.':
                //still a number...
                
                inputBytes.MoveNext();
                var trailing = inputBytes.ReadNumeric();
                Span<byte> tempBuffer = stackalloc byte[number.Length + trailing.Length + 1];
                number.CopyTo(tempBuffer);
                tempBuffer[number.Length] = (byte)'.';
                trailing.CopyTo(tempBuffer[(number.Length + 1)..]);
                return new NumericObject(sign * double.Parse(tempBuffer), begin,  inputBytes.CurrentOffset - begin);
            default:
                //we are done parsing the number I think...
                return new NumericObject(sign * double.Parse(number), begin, inputBytes.CurrentOffset - begin, isFraction);
        }
    }

    private static NameObject ParseNameObject(MemoryInputBytes inputBytes)
    {
        if (inputBytes.CurrentByte != '/')
        {
            throw new Exception("Name objects expected to start with a /");
        }

        var begin = inputBytes.CurrentOffset;
        inputBytes.MoveNext();

        var nameBytes = inputBytes.ReadAlpha();

        if (nameBytes.Length == 0)
        {
            throw new Exception("Could not parse name from name object. Read 0 Alpha bytes after /.");
        }
        
        var name = Encoding.ASCII.GetString(nameBytes);
        return new NameObject(name, begin, inputBytes.CurrentOffset - begin);
    }

    public static DirectObject ParseStringObject(MemoryInputBytes inputBytes)
    {
        Debug.Assert(inputBytes.CurrentChar is '(' or '<');
        var begin = inputBytes.CurrentOffset;
        ReadOnlySpan<byte> value;
        switch (inputBytes.CurrentChar)
        {
            case '(':
                value = inputBytes.ReadUntil(")"u8);
                break;
            case '<':
                value = inputBytes.ReadUntil(">"u8);
                break;
            default:
                throw new UnreachableException();
        }
        
        Debug.Assert(inputBytes.CurrentChar is ')' or '>');

        return new StringObject(inputBytes.Slice((int)begin + 1, (int)(inputBytes.CurrentOffset - begin)), begin, inputBytes.CurrentOffset - begin);
    }

    public static DirectObject ParseArrayObject(MemoryInputBytes inputBytes)
    {
        //Arrays in PDF are space delineated and can contain any object, including other arrays.
        //So essentially loop until we see an end of array token
        var begin = inputBytes.CurrentOffset;
        var objects = new List<DirectObject>();
        
        inputBytes.MoveNext(); //Move into the array
        while (inputBytes.CurrentChar != ']')
        {
            objects.Add(ParseDirectObject(inputBytes));
        }

        inputBytes.MoveNext(); //Move out
        
        return new ArrayObject(objects, begin, inputBytes.CurrentOffset - begin);
    }
    
    public static DirectObject ParseDirectObject(MemoryInputBytes inputBytes)
    {
        switch (inputBytes.CurrentChar)
        {
            case '<':
                if (inputBytes.Peek() == '<')
                {
                    var dictionary = ParseDictionaryObject(inputBytes);
                    return dictionary;
                }
                else
                {
                    return ParseStringObject(inputBytes);
                }
            case '/':
                return ParseNameObject(inputBytes);
            case '(':
                return ParseStringObject(inputBytes);
            case '[':
                return ParseArrayObject(inputBytes);
            case var digit when char.IsDigit(digit):
            case '+':
            case '-':
            case '.':
                return ParseNumericObject(inputBytes);
            case var other when char.IsWhiteSpace(other):
                inputBytes.SkipWhitespace();
                return ParseDirectObject(inputBytes);
            default:
                throw new Exception($"Unable to parse object");
        }
    }
}