using PDFParser.Parser.Document;

namespace PDFParser.Parser.Objects;

public class StreamObject : DirectObject
{
    //Note -- Content Streams
    //PDF Content Streams apart of the PDF syntax but have a unique syntax introduction called operator
    //Streams use Reverse Polish notation, so you see operands followed by operators
    //Operands are expressed as standard PDF objects -- i.e. Array Objects, Named Objects, Numeric Objects, String Objects
    //Operators are special codes that do specific operations, i.e. sets the type face, prints text, sets color, etc.
    //
    public ReadOnlyMemory<byte> Data { get; }
    
    public StreamObject(ReadOnlyMemory<byte> buffer, long offset, long length) : base(offset, length)
    {
        Data = buffer;
    }

    public void Parse()
    {
        //Parses a stream object... not sure what data structure this returns yet...
        throw new NotImplementedException();
    }
}