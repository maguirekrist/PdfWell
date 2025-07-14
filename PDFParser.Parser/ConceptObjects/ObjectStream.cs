using PDFParser.Parser.Objects;

namespace PDFParser.Parser.ConceptObjects;

public class ObjectStream
{
    //TODO: Handle Object Streams!
    
    public StreamObject Stream { get; }
    
    public ObjectStream(StreamObject obj)
    {
        Stream = obj;
    }
    
    public ObjectStream(ReadOnlyMemory<byte> buffer, DictionaryObject streamDictionary) : this(new StreamObject(buffer, streamDictionary))
    {
    }

    //Number of indirect objects stored in the stream.
    public int Count => (int)Stream.GetAs<NumericObject>("N").Value;
    //The byte offset in the decoded stream of the first compressed object.
    public int First => (int)Stream.GetAs<NumericObject>("First").Value;

    //A reference to another object stream, of which the current object stream is an extension.
    //Both streams are considered part of a collection of object streams. 
    public ReferenceObject? Extension => Stream.TryGetAs<ReferenceObject>("Extends");
}