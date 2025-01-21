using PDFParser.Parser.Document;

namespace PDFParser.Parser.Objects;

public class StreamObject : DirectObject
{
    //Note -- Content Streams
    //PDF Content Streams apart of the PDF syntax but have a unique syntax introduction called operator
    //Streams use Reverse Polish notation, so you see operands followed by operators
    //Operands are expressed as standard PDF objects -- i.e. Array Objects, Named Objects, Numeric Objects, String Objects
    //Operators are special codes that do specific operations, i.e. sets the type face, prints text, sets color, etc.
    

    //This class is simply a wrapper for the stream data... Data represents the contents between stream and endstream
    //The class user is responsible for making sense of the stream.

    public ReadOnlyMemory<byte> Data { get; }

    public StreamObject(ReadOnlyMemory<byte> buffer, long offset, long length) : base(offset, length)
    {
        Data = buffer;
    }
}