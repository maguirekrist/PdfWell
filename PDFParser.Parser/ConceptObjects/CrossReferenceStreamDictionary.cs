using PDFParser.Parser.Objects;

namespace PDFParser.Parser.ConceptObjects;

public class CrossReferenceStreamDictionary
{
    //Xref entries have 3 types
    //0 free object
    //1 objects that are not compressed.
    //2 objects that are in a compressed object stream.
    
    public StreamObject Stream { get; }
    
    public CrossReferenceStreamDictionary(StreamObject streamObject)
    {
        Stream = streamObject;
    }
    
    //Required - an array of integers representing the size of the fields in a single xref entry.
    //Always contains three integers.
    //The value of each integer shall be the number of bytes of the corresponding field.
    //The value of zero for an element in the W array indicates that the corresponding field shall not be present in the stream.
    //And the default value shall be used, if there is one.
    //0 is not a valid value for the second element.
    //0 for the first means the type field shall be defaulted to Type 1.
    public ArrayObject<DirectObject> BytePattern => Stream.GetAs<ArrayObject<DirectObject>>("W");
    //Optional - but required if there is another xref stream exists in the file. 
    //Holds the byte offset to the other xref stream. Useful for chain reading.
    public NumericObject? PreviousXrefStream => Stream.TryGetAs<NumericObject>("Prev");
    
    //Optional - an array containing a pair of integer for each subsection in this section.
    //The first integer shall be the first object number in the subsection.
    //The second integer shall be the number of entries in the subsection.
    public ArrayObject<DirectObject>? IndexArray => Stream.TryGetAs<ArrayObject<DirectObject>>("Index");

    public ReferenceObject? EncryptRef => Stream.TryGetAs<ReferenceObject>("Encrypt");

    public ArrayObject<DirectObject>? IDs => Stream.TryGetAs<ArrayObject<DirectObject>>("ID");

    public ReferenceObject? RootRef => Stream.TryGetAs<ReferenceObject>("Root");
    
    public NumericObject Size => Stream.GetAs<NumericObject>("Size");
}