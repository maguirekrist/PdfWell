using System.Buffers;
using System.Text;
using PDFParser.Parser.Document;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser;

public class PdfWriter : IDisposable
{
    private readonly ObjectTable _objectTable;
    private readonly FileStream _file;
    private readonly BufferedStream _streamWriter;

    public PdfWriter(ObjectTable objectTable, string path)
    {
        _objectTable = objectTable;
        _file = File.Create(path);
        _streamWriter = new BufferedStream(_file); //We should initialize this with capacity... or something. 
    }

    public void Write()
    {
        var offsetList = new List<(long offset, bool isFree)> { (0, true) };

        _streamWriter.Write(WriteHeader());

        foreach (var (reference, obj) in _objectTable)
        {
            offsetList.Add((_streamWriter.Position, false));    
            _streamWriter.Write(WriteObjectHeader(reference));
            _streamWriter.Write("\n"u8);
            _streamWriter.Write(WriteObject(obj));
            _streamWriter.Write("\nendobj\n\n"u8);
        }

        var startXref = _streamWriter.Position;
        _streamWriter.Write(WriteXref(offsetList));
        
        _streamWriter.Write(WriteTrailer(_objectTable));
        
        _streamWriter.Write("startxref\n"u8);
        _streamWriter.Write(Encoding.ASCII.GetBytes($"{startXref}\n"));
        
        _streamWriter.Write("%%EOF"u8);
        _streamWriter.Flush();
    }

    private static Span<byte> WriteHeader()
    {
        return "%PDF-1.7\n"u8.ToArray();
    }

    private static Span<byte> WriteTrailer(ObjectTable objectTable)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();

        bufferWriter.Write("trailer\n"u8);
        var dict = new Dictionary<NameObject, DirectObject>();
        dict.Add(new NameObject("Size"), new NumericObject(objectTable.Count));
        dict.Add(new NameObject("Root"), new ReferenceObject(objectTable.GetCatalogReference()));
        var dictObj = new DictionaryObject(dict, 0, 0);

        bufferWriter.Write(WriteDictionaryObject(dictObj));
        bufferWriter.Write("\n"u8);
        
        return bufferWriter.WrittenSpan.ToArray();
    }

    private static Span<byte> WriteXref(List<(long offset, bool isFree)> offsets)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        var offsetLen = offsets.Count;
        
        bufferWriter.Write("xref\n"u8);
        bufferWriter.Write(Encoding.ASCII.GetBytes($"0 {offsetLen}\n"));
        Span<byte> offsetRun = stackalloc byte[10];
        foreach (var (offset, isFree) in offsets)
        {
            var offsetAsAscii = Encoding.ASCII.GetBytes($"{offset}");

            for (var i = 9; i >= 0; i--)
            {
                var indexIntoAscii = 9 - i;
                if (indexIntoAscii < offsetAsAscii.Length)
                {
                    offsetRun[i] = offsetAsAscii[(offsetAsAscii.Length - 1) - indexIntoAscii];
                }
                else
                {
                    offsetRun[i] = (byte)'0';
                }
            }
            
            bufferWriter.Write(offsetRun);
            bufferWriter.Write(" "u8);
            bufferWriter.Write("00000 "u8);
            bufferWriter.Write(isFree ? "f"u8 : "n"u8);
            bufferWriter.Write("\n"u8);
        }
        
        return bufferWriter.WrittenSpan.ToArray();
    }

    private static Span<byte> WriteObjectHeader(IndirectReference reference)
    {
        return Encoding.ASCII.GetBytes($"{reference.ObjectNumber} 0 obj");
    }
    
    private static Span<byte> WriteObject(DirectObject pdfObject)
    {
        return pdfObject switch
        {
            ArrayObject<DirectObject> arrayObject => WriteArrayObject(arrayObject),
            BooleanObject booleanObject => WriteBooleanObject(booleanObject),
            StreamObject streamObject => WriteStreamObject(streamObject),
            DictionaryObject dictionaryObject => WriteDictionaryObject(dictionaryObject),
            NameObject nameObject => WriteNameObject(nameObject),
            NullObject nullObject => WriteNullObject(nullObject),
            NumericObject numericObject => WriteNumericObject(numericObject),
            ReferenceObject referenceObject => WriteReferenceObject(referenceObject),
            StringObject stringObject => WriteStringObject(stringObject),
            _ => throw new ArgumentOutOfRangeException(nameof(pdfObject))
        };
    }

    private static Span<byte> WriteNameObject(NameObject nameObject)
    {
        return Encoding.ASCII.GetBytes($"/{nameObject.Name}");
    }

    private static Span<byte> WriteReferenceObject(ReferenceObject referenceObject)
    {
        return Encoding.ASCII.GetBytes($"{referenceObject.Reference.ObjectNumber} {referenceObject.Reference.Generation} R");
    }

    private static Span<byte> WriteBooleanObject(BooleanObject booleanObject)
    {
        return booleanObject.Value ? "true"u8.ToArray() : "false"u8.ToArray();
    }

    private static Span<byte> WriteNullObject(NullObject nullObject)
    {
        return ("null"u8).ToArray();
    }
    
    private static Span<byte> WriteNumericObject(NumericObject numericObject)
    {
        return Encoding.ASCII.GetBytes($"{numericObject.Value}");
    }
    
    private static Span<byte> WriteStringObject(StringObject stringObject)
    {
        return stringObject.Data.Span.ToArray();
    }

    private static Span<byte> WriteDictionaryObject(DictionaryObject dictionaryObject)
    {
        var buffer = new ArrayBufferWriter<byte>();

        buffer.Write("<<"u8);

        foreach (var kvp in dictionaryObject.Dictionary)
        {
            buffer.Write(WriteNameObject(kvp.Key));
            buffer.Write(" "u8);
            buffer.Write(WriteObject(kvp.Value));
            buffer.Write(" "u8);
        }
        
        buffer.Write(">>"u8);
        return buffer.WrittenSpan.ToArray();
    }

    private static Span<byte> WriteArrayObject(ArrayObject<DirectObject> arrayObject)
    {
        var buffer = new ArrayBufferWriter<byte>();

        buffer.Write(Encoding.ASCII.GetBytes("[ ")); // Opening of PDF array

        foreach (var obj in arrayObject.Objects)
        {
            var span = WriteObject(obj); // This returns Span<byte>
            buffer.Write(span);
            buffer.Write(new byte[] { (byte)' ' }); // Space between elements
        }

        buffer.Write(Encoding.ASCII.GetBytes("]")); // Closing of PDF array

        return buffer.WrittenSpan.ToArray(); // or buffer.WrittenMemory.ToArray(
    }

    private static Span<byte> WriteStreamObject(StreamObject streamObject)
    {
        var buffer = new ArrayBufferWriter<byte>();

        buffer.Write(WriteDictionaryObject(streamObject));
        buffer.Write("\n"u8);
        buffer.Write("stream\n"u8);
        buffer.Write(streamObject.Data);
        buffer.Write("endstream"u8);
        
        return buffer.WrittenSpan.ToArray();
    }
    
    public void Dispose()
    {
        //Why does a memoryStream need a Flush? 
        // _memoryStream.Flush();
        // _memoryStream.Dispose();
        _streamWriter.Flush();
        _streamWriter.Dispose();
        _file.Dispose();
    }
}