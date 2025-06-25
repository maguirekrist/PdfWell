using System.Collections.ObjectModel;
using System.Diagnostics;
using PDFParser.Parser.Attributes;
using PDFParser.Parser.Document;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.ConceptObjects;

public class CrossReferenceStreamDictionary : StreamObject
{
    //Xref entries have 3 types
    //0 free object
    //1 objects that are not compressed.
    //2 objects that are in a compressed object stream.
    public CrossReferenceStreamDictionary(StreamObject streamObject) : base(streamObject)
    {
    }
    
    //Required - an array of integers representing the size of the fields in a single xref entry.
    //Always contains three integers.
    //The value of each integer shall be the number of bytes of the corresponding field.
    //The value of zero for an element in the W array indicates that the corresponding field shall not be present in the stream.
    //And the default value shall be used, if there is one.
    //0 is not a valid value for the second element.
    //0 for the first means the type field shall be defaulted to Type 1.
    public ArrayObject<DirectObject> BytePattern => GetAs<ArrayObject<DirectObject>>("W");
    //Optional - but required if there is another xref stream exists in the file. 
    //Holds the byte offset to the other xref stream. Useful for chain reading.
    public NumericObject? PreviousXrefStream => TryGetAs<NumericObject>("Prev");
    
    //Optional - an array containing a pair of integer for each subsection in this section.
    //The first integer shall be the first object number in the subsection.
    //The second integer shall be the number of entries in the subsection.
    public ArrayObject<DirectObject>? IndexArray => TryGetAs<ArrayObject<DirectObject>>("Index");
    public NumericObject Size => GetAs<NumericObject>("Size");
}