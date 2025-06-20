using PDFParser.Parser.Objects;

namespace PDFParser.Parser.ConceptObjects;

public class ObjectStream : StreamObject
{
    //TODO: Handle Object Streams!
    public ObjectStream(StreamObject obj) : base(obj)
    {
    }
    
    public ObjectStream(ReadOnlyMemory<byte> buffer, DictionaryObject streamDictionary) : base(buffer, streamDictionary)
    {
    }

    //Number of indirect objects stored in the stream.
    public int Count => (int)GetAs<NumericObject>("N").Value;
    //The byte offset in the decoded stream of the first compressed object.
    public int First => (int)GetAs<NumericObject>("First").Value;

    //A reference to another object stream, of which the current object stream is an extension.
    //Both streams are considered part of a collection of object streams. 
    public ReferenceObject? Extension => TryGetAs<ReferenceObject>("Extends");
}