using PDFParser.Parser.ConceptObjects;
using PDFParser.Parser.Encryption;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Factories;

public static class XrefTableFactory
{


    public static Dictionary<IndirectReference, int> Build(CrossReferenceStreamDictionary xrefDictionary)
    {
        Dictionary<IndirectReference, int> xrefTable = new();
        Dictionary<IndirectReference, List<int>> compressedObjectMap = new();
        List<IndirectReference> freeObjects = new();
        
        var bytePattern = xrefDictionary.BytePattern;
        var field1Length = (int)((NumericObject)bytePattern.Objects[0]).Value;
        var field2Length = (int)((NumericObject)bytePattern.Objects[1]).Value;
        var field3Length = (int)((NumericObject)bytePattern.Objects[2]).Value;
        var rowLength = field1Length + field2Length + field3Length;

        var objectCounter = xrefDictionary.IndexArray != null ? (int)xrefDictionary.IndexArray.GetAs<NumericObject>(0).Value : 0;

        var decodedStream = CompressionHandler.Decompress(xrefDictionary).Span;
        
        for (var i = 0; i < decodedStream.Length; i += rowLength)
        {
            var rowData = decodedStream.Slice(i, rowLength);
            var entryType = rowData.Slice(0, field1Length);
            var fieldTwo = rowData.Slice(field1Length, field2Length);
            var fieldThree = rowData.Slice(field1Length + field2Length, field3Length);
            
            switch (BinaryHelper.ReadVariableIntBigEndian(entryType))
            {
                case 0:
                    var nextFreeObjNumber = BinaryHelper.ReadVariableIntBigEndian(fieldTwo);
                    var genNumber = BinaryHelper.ReadVariableIntBigEndian(fieldThree);
                    freeObjects.Add(new IndirectReference(objectCounter++, genNumber));
                    break;
                case 1:
                    var byteOffsetOfObject = BinaryHelper.ReadVariableIntBigEndian(fieldTwo);
                    //var generationNumber = BinaryHelper.ReadVariableIntBigEndian(fieldThree.Span)
                    xrefTable.Add(new IndirectReference(objectCounter++), byteOffsetOfObject);
                    break;
                case 2:
                    
                    //Object number of the stream the current object is in...
                    var streamObjNumber = BinaryHelper.ReadVariableIntBigEndian(fieldTwo);
                    var indexInStream = BinaryHelper.ReadVariableIntBigEndian(fieldThree);
                    var reference = new IndirectReference(streamObjNumber);
                    compressedObjectMap.TryGetValue(reference, out var list);
                    if (list == null)
                    {
                        list = new();
                    }
                    list.Add(indexInStream);
                    compressedObjectMap[reference] = list;

                    xrefTable.TryGetValue(new IndirectReference(streamObjNumber), out var streamObjOffset);
                    xrefTable.Add(new IndirectReference(objectCounter++), streamObjOffset);
                    break;
            }
        }

        return xrefTable;
    }
}