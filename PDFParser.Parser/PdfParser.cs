using System.Buffers.Binary;
using System.Diagnostics;
using System.Text;
using PDFParser.Parser.ConceptObjects;
using PDFParser.Parser.Crypt;
using PDFParser.Parser.Document;
using PDFParser.Parser.Encryption;
using PDFParser.Parser.Exceptions;
using PDFParser.Parser.Factories;
using PDFParser.Parser.IO;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser;

public class PdfParser
{
    private readonly MemoryInputBytes _memoryReader;
    private CrossReferenceTable? _crossReferenceTable = null;
    private Trailer? _trailer;
    private readonly ObjectTable _objectTable = new();
    private IndirectReference? _encryptionRef;
    private EncryptionDictionary? _encryption;
    private ArrayObject<DirectObject>? _fileIdArray;
    private EncryptionHandler? _encryptionHandler;
    

    public PdfParser(byte[] pdfData, IMatcher? matcherStrategy = null)
    {
        _memoryReader = new MemoryInputBytes(pdfData, matcherStrategy);
    }

    public PdfParser(MemoryInputBytes inputBytes)
    {
        _memoryReader = inputBytes;
    }
    
    public PdfDocument Parse()
    {
        //There are two ways to begin parsing a PDF depending on the use case.
        //1. Linearized parsing - this is useful for large pdfs, if your streaming content from a server, or do not want to load the whole pdf in memory at once (SUPER LARGE PDFs). 
        //This is really only useful when you are building a PDF application for web or regular users, not necessary for PDF processors like this software (where servers with TONs of RAM are usually opening up the whole file in mem).
        //2. Trailer Parsing - Since the whole file is in memory anyway, for example in this software, we can just jump straight to the main trailer which tells us all we need to know to start/decrypt the PDF. 
        //This is how we'll do it.
        
        var isLinearized = IsLinearized();

        _memoryReader.Seek(0);
        
        var startXref = FindStartXrefOffset();
        _memoryReader.Seek(0);
        
        var xrefTable = ParseXrefs(_memoryReader, startXref);
        _trailer = ParseTrailer(_memoryReader);
        if (_encryptionRef.HasValue)
        {
            xrefTable.TryGetValue(_encryptionRef.Value, out var offsetVal);
            var obj = ParseObjectByOffset(_memoryReader, offsetVal, out var _);
            if (obj is DictionaryObject dict)
            {
                var encryptionDict = new EncryptionDictionary(dict);
                _encryptionHandler = new EncryptionHandler(encryptionDict, _fileIdArray!);
            }
        }
        
        foreach (var (xRef, offset) in xrefTable)
        {
            if (_objectTable.ContainsKey(xRef)) continue;
            
            try
            {
                var obj = ParseObjectByOffset(_memoryReader, offset, out var key);
                if (obj is ObjectStream objStream)
                {
                    //Parse Object Stream
                    ParseObjectStream(xRef, objStream);
                }
            
                _objectTable.TryAdd(key, obj);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occured trying to parse object with ref: {xRef}", ex);
            }
        }
        
        return new PdfDocument(_objectTable, _trailer, _encryptionHandler) { IsLinearized = isLinearized };
    }

    private void ParseLinearized()
    {
        //TODO: Parse Hint Table
        //Hint Tables are PDF Dictionaries with /Linearized key
        //This is apart of the PDF technology called "Fast Web View"
        //L - The total file length (in bytes)
        //H - An Array of integers containing offset and length information of hint stream (obj locations and page content)
        //O - First Page Object Number
        //E - end of first page (byte offset)
        //N - Number of pages
        //T - Offset of First Page Cross-Reference Table
        var linearizedDict = TryGetLinearizedDictionary();
        if (linearizedDict != null)
        {
            var hintTableStart = linearizedDict.HintOffsets.GetAs<NumericObject>(0);
            var hintTableLength = linearizedDict.HintOffsets.GetAs<NumericObject>(1);
            _memoryReader.Seek((int)hintTableStart.Value);
            var objectLine = _memoryReader.ReadLine();
            var hintDictionary = ParseDirectObject(_memoryReader);
            
            // var hintTableSlice = _memoryReader.Slice((int)hintTableStart.Value, (int)hintTableLength.Value);
            // var hintTableReader = new MemoryInputBytes(hintTableSlice);
            // var hintStreamDict = GetNextObject(hintTableReader);

            if (hintDictionary is not StreamObject dict) throw new Exception("WTF!!");
            
        }
    }
    
    private bool IsLinearized()
    {
        var offset = _memoryReader.FindFirstPatternOffset("/Linearized"u8, 4_000_000);

        //_memoryReader.RewindUntil("obj"u8);
        
        //_memoryReader.Seek(0);
        
        return offset != null;
    }

    private LinearizedDictionary? TryGetLinearizedDictionary()
    {
        var firstObject = GetNextObject(_memoryReader);

        if (firstObject is DictionaryObject dictionaryObject)
        {
            if (dictionaryObject.Dictionary.Keys.Any(x => x.Name == "Linearized"))
            {
                //TODO: Implement Dynamic Dictionary Mapping.
                var linearizedDict = dictionaryObject.TryMapTo<LinearizedDictionary>();
                return linearizedDict;
                //This is a linearized hint table
                //For a test, let's grab the hint stream and giure out what it is....
                // var hintArray = dictionaryObject.GetAs<ArrayObject>("H");
                // var offset = hintArray[0] as NumericObject ?? throw new UnreachableException();
                // var length = hintArray[1] as NumericObject ?? throw new UnreachableException();
                //
                // _memoryReader.Seek((int)offset.Value);
                // var hintStream = _memoryReader.Slice((int)offset.Value, (int)length.Value);
                //
                // //Console.WriteLine(Encoding.ASCII.GetString(hintStream.Span));
                //
                // var hintStreamObject = GetNextObject(_memoryReader);
                //
                // if (hintStreamObject is DictionaryObject { HasLength: true, Stream: not null } streamObject)
                // {
                //     var reader = streamObject.Stream.Reader;
                //     //Console.WriteLine(streamObject.Stream.DecodedStream);
                // }
            }
        }

        return null;
    }
    
    private DirectObject GetNextObject(MemoryInputBytes inputBytes)
    {
        var objOffset = inputBytes.FindFirstPatternOffset("obj"u8);
        inputBytes.SkipWhitespace();
        return ParseDirectObject(inputBytes);
    }
    
    //TODO:
    private string FindPdfVersion(MemoryInputBytes inputBytes)
    {
        //Get the first line
        throw new NotImplementedException();
    }

    private Trailer? ParseTrailer(MemoryInputBytes inputBytes)
    {
        ReadOnlySpan<byte> trailerMarker = "trailer"u8;
        var trailerOffset = inputBytes.FindFirstPatternOffset(trailerMarker);

        if (!trailerOffset.HasValue)
        {
            return null;
        }
        
        inputBytes.MoveNext();
        var dictionary = ParseDictionaryObject(inputBytes);
        
        return new Trailer(dictionary);
    }

    public long FindStartXrefOffset()
    {
        //A PDF can have multiple startxref (incremental update feature in the spec). 
        //At load time, ONLY the LAST startxref matters and thats the boy we should load. 
        var readStart = _memoryReader.Length - 2000;
        _memoryReader.Seek(readStart > 0 ? readStart : 0);
        
        ReadOnlySpan<byte> startXrefMarker = "startxref"u8;
        var startXref = _memoryReader.FindFirstPatternOffset(startXrefMarker) ?? throw new Exception("StartXref was not found.");
        _memoryReader.NextLine();
        var nextLine = _memoryReader.ReadLine();
        long.TryParse(nextLine, out var result);
        return result;
    }

    private void ParseObjectStreams()
    {
        var objStreams = _objectTable
            .Where(kv => kv.Value is ObjectStream) // Filter condition
            .ToDictionary(kv => kv.Key, kv => (ObjectStream)kv.Value);
        foreach (var (reference, objStream) in objStreams)
        {
            ParseObjectStream(reference, objStream);
        }
    }

    private void ParseObjectStream(IndirectReference reference, ObjectStream objStream)
    {
        var decryptedStream = objStream.Data;
        if (_encryptionHandler != null)
        {
            decryptedStream = _encryptionHandler.Decrypt(objStream.Data, reference);
        }
        //Decoding should be handled outside of the class....
        var decodedStream = CompressionHandler.Decompress(decryptedStream, objStream.Filter, objStream.DecoderParams);
        var streamReader = new MemoryInputBytes(decodedStream);

        var beginOffset = objStream.First;
        var objectsInStream = new Dictionary<IndirectReference, int>();
        
        for (var i = 0; i < objStream.Count; i++)
        {
            var objNumberBytes = streamReader.ReadNumeric();
            int.TryParse(objNumberBytes, out var objNumber);
            streamReader.SkipWhitespace();
            int.TryParse(streamReader.ReadNumeric(), out var objOffset);
            streamReader.SkipWhitespace();
            objectsInStream.Add(new IndirectReference(objNumber), objOffset);
        }

        foreach (var (objRef, offset) in objectsInStream)
        {
            try
            {
                streamReader.Seek(beginOffset + offset);
                var obj = ParseDirectObject(streamReader);
                _objectTable.Add(objRef, obj);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"An error occured trying to parse object in object stream, object ref in stream: {objRef}. \n Object stream: \n {Encoding.ASCII.GetString(decodedStream.Span)}", ex);
            }
        }
    }
    
    private Dictionary<IndirectReference, long> ParseXrefs(MemoryInputBytes inputBytes, long startXref)
    {
        inputBytes.Seek(startXref);
        var xrefTable = new Dictionary<IndirectReference, long>();

        ReadOnlySpan<byte> xrefMarker = "xref"u8;
        if (!inputBytes.Match(xrefMarker))
        {
            var xrefStream = ParseXrefStream(inputBytes);
            if (xrefStream.HasKey("Encrypt"))
            {
                _encryptionRef = xrefStream.GetAs<ReferenceObject>("Encrypt").Reference;
            }

            if (xrefStream.HasKey("ID"))
            {
                _fileIdArray = xrefStream.TryGetAs<ArrayObject<DirectObject>>("ID");
            }
            
            return ResolveXrefTable(xrefStream);
        }
        
        inputBytes.NextLine();
        
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
        
        //var xrefTable = new CrossReferenceTable(xrefTable, startXref, inputBytes.CurrentOffset - startXref);
        return xrefTable;
    }
    
    private CrossReferenceStreamDictionary ParseXrefStream(MemoryInputBytes inputBytes)
    {
        
        var objectLine = inputBytes.ReadLine();
        var objectLineStr = Encoding.ASCII.GetString(objectLine);
        if (!objectLineStr.Contains("obj")) throw new UnreachableException();
        
        //This is an xref stream
        var xrefStreamObj = ParseDirectObject(inputBytes);
        if (xrefStreamObj is not StreamObject xrefStreamDict)
            throw new Exception($"Expected an xref stream object, got something else: {xrefStreamObj.GetType()}");

        var type = xrefStreamDict.GetAs<NameObject>("Type");
                
        if (!type.Name.Equals("XRef", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Expected a xref stream object but got something else");
        }

        var encryptRef = xrefStreamDict.TryGetAs<ReferenceObject>("Encrypt");
        if (encryptRef != null)
        {
            //Parse and build encryption dictionary object
            //throw new Exception("WOW!");
        }
        
        //This is a xref stream object!
        return new CrossReferenceStreamDictionary(xrefStreamDict);
    }

    private Dictionary<IndirectReference, long> ResolveXrefTable(CrossReferenceStreamDictionary xrefStream)
    {
        var xrefTable = new Dictionary<IndirectReference, long>();
        
        //Check for Previous XRef stream
        var prevXrefStream = xrefStream.PreviousXrefStream;
        while (prevXrefStream != null)
        {
            _memoryReader.Seek((int)prevXrefStream.Value);
            var prevStream = ParseXrefStream(_memoryReader);
            var tempTable = XrefTableFactory.Build(prevStream);
            xrefTable = xrefTable.Concat(tempTable).ToDictionary();
            prevXrefStream = prevStream.PreviousXrefStream;
        }

        var mainTable = XrefTableFactory.Build(xrefStream);
        xrefTable = xrefTable.Concat(mainTable).ToDictionary();
        return xrefTable;
    }

    private DirectObject ParseObjectByOffset(MemoryInputBytes inputBytes, long offset, out IndirectReference objectKey)
    {
        inputBytes.Seek(offset);
        var objectLine = inputBytes.ReadLine();
        
        objectKey = ParseObjectLine(objectLine);

        if (_objectTable.TryGetValue(objectKey, out var dirObj))
        {
            return dirObj;
        }
        
        try
        {
            return ParseDirectObject(inputBytes);
        }
        catch (Exception ex)
        {
            var objectSpan = inputBytes.Slice((int)offset, (int)(inputBytes.CurrentOffset - offset)).Span;
            throw new Exception($"Exception was thrown trying to parse object: {Encoding.ASCII.GetString(objectSpan)}", ex);
        }
    }
    
    private DirectObject ParseObjectByReference(IndirectReference objectReference)
    {
        if (_objectTable.TryGetValue(objectReference, out var dirObject)) return dirObject;
        if (_crossReferenceTable == null) throw new Exception("Parser Bad State! Cross Reference Table is NULL!");
        
        var offset = _crossReferenceTable.ObjectOffsets[objectReference];
        dirObject = ParseObjectByOffset(_memoryReader, offset, out _);
        return dirObject;
    }

    private IndirectReference ParseObjectLine(ReadOnlySpan<byte> objectLine)
    {
        var objectLineStr = Encoding.ASCII.GetString(objectLine);
        if (!objectLineStr.Contains("obj")) throw new ArgumentException($"Invalid object line: {objectLineStr}");
        var index = 0;
        while (!char.IsWhiteSpace((char)objectLine[index]))
        {
            index++;
        }

        int.TryParse(objectLine.Slice(0, index), out var objNumber);

        return new IndirectReference(objNumber);
    }
    
    private static ReadOnlyMemory<byte> GetStreamBuffer(MemoryInputBytes inputBytes, int length)
    {
        inputBytes.SkipWhitespace();
        var key = "stream"u8;
        if (!inputBytes.Match(key))
        {
            throw new UnreachableException();
        }
        inputBytes.Move(key.Length);

        //Beginning of stream
        inputBytes.SkipWhitespace();
        var begin = inputBytes.CurrentOffset;
        return inputBytes.Slice((int)begin, length);
    }

    private DictionaryObject ParseDictionaryObject(MemoryInputBytes inputBytes)
    {
        //We are outside of the dictionary
        var beginOffset = inputBytes.CurrentOffset;
        var dictionary = new Dictionary<NameObject, DirectObject>();

        if (!inputBytes.Match("<<"u8))
        {
            throw new Exception($"Expected dictionary begin token (<<) read: {inputBytes.CurrentChar}");
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

    public DirectObject ParseNumericObject(MemoryInputBytes inputBytes)
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

                    var reference = new IndirectReference(int.Parse(number), int.Parse(genNumber));
                    // var offset = inputBytes.CurrentOffset;
                    // var directObject = ParseObjectByReference(reference);
                    // inputBytes.Seek(offset);
                    
                    return new ReferenceObject(reference, begin, inputBytes.CurrentOffset - begin);
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

    private NameObject ParseNameObject(MemoryInputBytes inputBytes)
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
        switch (inputBytes.CurrentChar)
        {
            case '(':
                do
                {
                    inputBytes.ReadUntil(")"u8);
                } while (inputBytes.LookBehind(2) is 92);
                break;
            case '<':
                inputBytes.ReadUntil(">"u8);
                break;
            default:
                throw new UnreachableException();
        }
        
        return new StringObject(inputBytes.Slice((int)begin, (int)(inputBytes.CurrentOffset - begin)), begin, inputBytes.CurrentOffset - begin);
    }

    private DirectObject ParseArrayObject(MemoryInputBytes inputBytes)
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
        
        //TODO: Handle the generic parameter to allow for more type support
        //TODO: Can PDF Arrays be of multiple types? 
        return new ArrayObject<DirectObject>(objects, begin, inputBytes.CurrentOffset - begin);
    }

    private BooleanObject ParseBooleanObject(MemoryInputBytes inputBytes, bool parseTrue)
    {
        var begin = inputBytes.CurrentOffset;
        if (inputBytes.Match(parseTrue ? "true"u8 : "false"u8))
        {
            inputBytes.Move(4);
            return new BooleanObject(parseTrue, begin, inputBytes.CurrentOffset - begin);
        }

        throw new UnreachableException();
    }

    private NullObject ParseNullObject(MemoryInputBytes inputBytes)
    {
        var begin = inputBytes.CurrentOffset;
        if (inputBytes.Match("null"u8))
        {
            inputBytes.Move(4);
            return new NullObject(begin, inputBytes.CurrentOffset - begin);
        }

        throw new UnreachableException();
    }

    private StreamObject ParseStreamObject(MemoryInputBytes inputBytes, DictionaryObject streamDictionary)
    {
        var lengthObject = streamDictionary.GetAs<NumericObject>("Length");
        var streamLength = (int)lengthObject.Value;
        var streamBuffer = GetStreamBuffer(inputBytes, streamLength);
        inputBytes.Seek(inputBytes.CurrentOffset + streamLength);
        inputBytes.SkipWhitespace();
        var endStreamLine = inputBytes.ReadLine();
        Debug.Assert(endStreamLine.SequenceEqual("endstream"u8));

        return streamDictionary.Type?.Name switch
        {
            "ObjStm" => new ObjectStream(streamBuffer, streamDictionary),
            _ => new StreamObject(streamBuffer, streamDictionary)
        };
    }
    
    public DirectObject ParseDirectObject(MemoryInputBytes inputBytes)
    {
        try
        {
            switch (inputBytes.CurrentChar)
            {
                case '<':
                    if (inputBytes.Peek() == '<')
                    {
                        var dictionary = ParseDictionaryObject(inputBytes);
                        //TODO: Make into it's own function? Repeated code.
                        inputBytes.SkipWhitespace();
                        if (dictionary.HasLength && inputBytes.Match("stream"u8))
                        {
                            return ParseStreamObject(inputBytes, dictionary);
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
        catch (Exception ex)
        {
            //var preSlice = Encoding.ASCII.GetString(inputBytes.Slice((int)startingOffset, (int)(startingOffset + inputBytes.CurrentOffset)).Span);
            throw new Exception($"Error parsing object!", ex);
        }
    }
}