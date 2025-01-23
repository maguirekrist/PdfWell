using System.Diagnostics;
using System.IO.Compression;
using PDFParser.Parser.Document;
using PDFParser.Parser.IO;

namespace PDFParser.Parser.Objects;

public enum StreamFilter
{
    None,
    Flate
}

public class StreamObject : DirectObject
{
    //Note -- Content Streams
    //PDF Content Streams apart of the PDF syntax but have a unique syntax introduction called operator
    //Streams use Reverse Polish notation, so you see operands followed by operators
    //Operands are expressed as standard PDF objects -- i.e. Array Objects, Named Objects, Numeric Objects, String Objects
    //Operators are special codes that do specific operations, i.e. sets the type face, prints text, sets color, etc.
    

    //This class is simply a wrapper for the stream data... Data represents the contents between stream and endstream
    //The class user is responsible for making sense of the stream.

    public ReadOnlyMemory<byte> Data { get; } //This is Raw Data.
    
    public StreamFilter Encoding { get; } //Filter used

    public StreamObject(ReadOnlyMemory<byte> buffer, StreamFilter encoding, long offset, long length) : base(offset, length)
    {
        Data = buffer;
        Encoding = encoding;
    }

    public MemoryInputBytes GetReader()
    {
        switch (Encoding)
        {
            case StreamFilter.None:
                return new MemoryInputBytes(Data);
            case StreamFilter.Flate:
            {
                var rawBytes = Data.ToArray()[2..];
                // Console.WriteLine(BitConverter.ToString(rawBytes));
                using var memoryStream = new MemoryStream(rawBytes);
                using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
                using var reader = new StreamReader(deflateStream);
                var decoded = reader.ReadToEnd();
                //Console.WriteLine(decoded);
                return new MemoryInputBytes(System.Text.Encoding.ASCII.GetBytes(decoded));
            }
            default:
                throw new UnreachableException();
        }
    }
}