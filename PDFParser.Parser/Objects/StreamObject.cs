namespace PDFParser.Parser.Objects;

public class StreamObject : DirectObject
{
    public ReadOnlyMemory<byte> Data { get; }
    
    public StreamObject(ReadOnlyMemory<byte> buffer, long offset, long length) : base(offset, length)
    {
        Data = buffer;
    }
}