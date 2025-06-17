using System.Diagnostics;
using PDFParser.Parser.Attributes;
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

    public void GetEntries()
    {
        var bytePattern = BytePattern;
        var field1Length = (int)((NumericObject)bytePattern.Objects[0]).Value;
        var field2Length = (int)((NumericObject)bytePattern.Objects[1]).Value;
        var field3Length = (int)((NumericObject)bytePattern.Objects[2]).Value;
        var rowLength = field1Length + field2Length + field3Length;
        for (var i = 0; i < DecodedStream.Length; i += rowLength)
        {
            var rowData = DecodedStream.Slice(i, rowLength);
            var entryType = rowData.Slice(0, field1Length);
            var fieldTwo = rowData.Slice(field1Length, field2Length);
            var fieldThree = rowData.Slice(field1Length + field2Length, field3Length);
            
            switch (BinaryHelper.ReadVariableIntBigEndian(entryType.Span))
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    //Yay!
                    var objectNumber = BinaryHelper.ReadVariableIntBigEndian(fieldTwo.Span);
                    break;
            }
        }
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
    public ArrayObject<NumericObject>? IndexArray => TryGetAs<ArrayObject<NumericObject>>("Index");
    public NumericObject Size => GetAs<NumericObject>("Size");
}