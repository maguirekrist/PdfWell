using System.Buffers;
using System.Text;
using PDFParser.Parser.ConceptObjects;
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
        _streamWriter.Write(WriteHeader());

        foreach (var (reference, obj) in _objectTable)
        {
            _streamWriter.Write(WriteObjectHeader(reference));
            _streamWriter.Write("\n"u8);
            _streamWriter.Write(WriteObject(obj));
            _streamWriter.Write("\nendobj\n\n"u8);
        }
        
        _streamWriter.Write(WriteXref());
        _streamWriter.Flush();
    }

    private static Span<byte> WriteHeader()
    {
        return "%PDF-1.7\n"u8.ToArray();
    }

    private static Span<byte> WriteXref()
    {
        return "xref"u8.ToArray();
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