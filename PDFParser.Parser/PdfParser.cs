using System.Diagnostics;
using System.Text;
using PDFParser.Parser.Exceptions;
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
        var firstObject = GetNextObject(_memoryReader);

        if (firstObject is DictionaryObject dictionaryObject)
        {
            if (dictionaryObject.Dictionary.Keys.Any(x => x.Name == "Linearized"))
            {
                //This is a linearized hint table
                //For a test, let's grab the hint stream and giure out what it is....
                var hintArray = dictionaryObject.GetAs<ArrayObject>("H");
                var offset = hintArray[0] as NumericObject ?? throw new UnreachableException();
                var length = hintArray[1] as NumericObject ?? throw new UnreachableException();

                _memoryReader.Seek((int)offset.Value);
                var hintStream = _memoryReader.Slice((int)offset.Value, (int)length.Value);

                Console.WriteLine(Encoding.ASCII.GetString(hintStream.Span));
                
                var hintStreamObject = GetNextObject(_memoryReader);

                if (hintStreamObject is DictionaryObject { IsStream: true, Stream: not null } streamObject)
                {
                    var reader = streamObject.Stream.GetReader();
                    Console.WriteLine(streamObject.Stream.DecodedStream);
                }
            }
        }
        
        // if (IsLinearized(_memoryReader))
        // {
        //     //TODO: Parse Hint Table
        //     //Hint Tables are PDF Dictionaries with /Linearized key
        //     //This is apart of the PDF technology called "Fast Web View"
        //     //L - The total file length (in bytes)
        //     //H - An Array of integers containing offset and length information of hint stream (obj locations and page content)
        //     //O - First Page Object Number
        //     //E - end of first page (byte offset)
        //     //N - Number of pages
        //     //T - Offset of First Page Cross-Reference Table
        //     throw new Exception("WTF!");
        // }
        
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

    private static bool IsLinearized(MemoryInputBytes inputBytes)
    {
        var offset = inputBytes.FindFirstPatternOffset("/Linearized"u8, 4_000_000);

        inputBytes.RewindUntil("obj"u8);
        
        inputBytes.Seek(0);
        
        return offset != null;
    }

    private static DirectObject GetNextObject(MemoryInputBytes inputBytes)
    {
        var objOffset = inputBytes.FindFirstPatternOffset("obj"u8);
        inputBytes.SkipWhitespace();
        return ParseDirectObject(inputBytes);
    }
    
    private static string FindPdfVersion(MemoryInputBytes inputBytes)
    {
        //Get the first line
        throw new NotImplementedException();
    }

    private static long FindStartTrailer(MemoryInputBytes inputBytes)
    {
        ReadOnlySpan<byte> trailerMarker = "trailer"u8;
        var offset = inputBytes.FindFirstPatternOffset(trailerMarker);

        return offset ?? 0;
    }

    private static long FindStartXrefOffset(MemoryInputBytes inputBytes)
    {
        ReadOnlySpan<byte> startXrefMarker = "startxref"u8;
        var startXref = inputBytes.FindFirstPatternOffset(startXrefMarker) ?? throw new Exception("StartXref was not found.");
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

    private static DirectObject ParseObjectByOffset(
        MemoryInputBytes inputBytes,
        long offset
        )
    {
        inputBytes.Seek(offset);
        var objectLine = inputBytes.ReadLine();

        try
        {
            return ParseDirectObject(inputBytes);
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception was thrown trying to parse object: {Encoding.ASCII.GetString(objectLine)}", ex);
        }
    }
    
    private static DirectObject ParseObjectByReference(
        MemoryInputBytes inputBytes,
        IndirectReference objectReference,
        CrossReferenceTable table)
    {
        // 1st get Byte Offset from Cross Reference Table
        var offset = table.ObjectOffsets[objectReference];
        return ParseObjectByOffset(inputBytes, offset);
    }

    private static StreamObject ParseStreamObject(MemoryInputBytes inputBytes, StreamFilter encoding, int length)
    {
        //Move to the next line
        //inputBytes.MoveNext();
        
        //var streamBegin = inputBytes.ReadLine();
        inputBytes.SkipWhitespace();
        if (!inputBytes.Match("stream"u8))
        {
            throw new UnreachableException();
        }

        //Beginning of stream
        inputBytes.SkipWhitespace();
        var begin = inputBytes.CurrentOffset;
        return new StreamObject(inputBytes.Slice((int)begin, length), encoding, begin, length);
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
                    try
                    {
                        var valueObject = ParseDirectObject(inputBytes);
                        dictionary.Add(nameObject, valueObject);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            $"An exception occured while parsing dictionary value for key /{nameObject.Name}", ex);
                    }
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

        var nameBytes = inputBytes.ReadName();

        if (nameBytes.Length == 0)
        {
            throw new Exception("Could not parse name from name object. Read 0 Alpha bytes after /.");
        }
        
        var name = Encoding.ASCII.GetString(nameBytes);
        return new NameObject(name, begin, inputBytes.CurrentOffset - begin);
    }

    public static StringObject ParseStringObject(MemoryInputBytes inputBytes)
    {
        Debug.Assert(inputBytes.CurrentChar is '(' or '<');
        var begin = inputBytes.CurrentOffset;
        // ReadOnlySpan<byte> value;
        switch (inputBytes.CurrentChar)
        {
            case '(':
                inputBytes.ReadUntil(")"u8);
                
                //Check if Escaped
                var escapeKey = inputBytes.LookBehind(2);

                if (escapeKey is 92)
                {
                    inputBytes.ReadUntil(")"u8);
                }
                break;
            case '<':
                inputBytes.ReadUntil(">"u8);
                break;
            default:
                throw new UnreachableException();
        }

        return new StringObject(inputBytes.Slice((int)begin, (int)(inputBytes.CurrentOffset - begin)), begin, inputBytes.CurrentOffset - begin);
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
            inputBytes.SkipWhitespace();
        }

        inputBytes.MoveNext(); //Move out
        
        return new ArrayObject(objects, begin, inputBytes.CurrentOffset - begin);
    }

    private static BooleanObject ParseBooleanObject(MemoryInputBytes inputBytes, bool parseTrue)
    {
        var begin = inputBytes.CurrentOffset;
        if (inputBytes.Match(parseTrue ? "true"u8 : "false"u8))
        {
            return new BooleanObject(parseTrue, begin, inputBytes.CurrentOffset - begin);
        }

        throw new UnreachableException();
    }

    private static NullObject ParseNullObject(MemoryInputBytes inputBytes)
    {
        var begin = inputBytes.CurrentOffset;
        if (inputBytes.Match("null"u8))
        {
            return new NullObject(begin, inputBytes.CurrentOffset - begin);
        }

        throw new UnreachableException();
    }

    private static void HandleStreamDictionary(MemoryInputBytes inputBytes, DictionaryObject streamDictionary)
    {
        //TODO: Read the stream
        var lengthObject = streamDictionary["Length"];
        var filterObject = streamDictionary["Filter"];

        StreamFilter encoding = StreamFilter.None;
        if (filterObject != null && filterObject is NameObject filter)
        {
            var filterName = filter.Name;
            switch (filterName)
            {
                case Filters.FlateEncoding:
                    encoding = StreamFilter.Flate;
                    break;
            }
        }
                
        if (lengthObject is NumericObject lengthNumeric)
        {
            var streamLength = (int)lengthNumeric.Value;
            streamDictionary.Stream = ParseStreamObject(inputBytes, encoding, (int)lengthNumeric.Value);
            inputBytes.Seek(inputBytes.CurrentOffset + streamLength);
            inputBytes.SkipWhitespace();
            var endStreamLine = inputBytes.ReadLine();
            Debug.Assert(endStreamLine.SequenceEqual("endstream"u8));
        }
        else
        {
            throw new UnreachableException();
        }
    }
    
    public static DirectObject ParseDirectObject(MemoryInputBytes inputBytes)
    {
        switch (inputBytes.CurrentChar)
        {
            case '<':
                if (inputBytes.Peek() == '<')
                {
                    var dictionary = ParseDictionaryObject(inputBytes);
                    if (dictionary.IsStream)
                    {
                        HandleStreamDictionary(inputBytes, dictionary);
                    }
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
            case 't':
            case 'f':
                return ParseBooleanObject(inputBytes, inputBytes.CurrentChar == 't');
            case 'n':
                //null object, which is odd but some pdf's have them
                return ParseNullObject(inputBytes);
            case var other when char.IsWhiteSpace(other):
                inputBytes.SkipWhitespace();
                return ParseDirectObject(inputBytes);
            default:
                throw new UnexpectedTokenException(inputBytes);
        }
    }
}