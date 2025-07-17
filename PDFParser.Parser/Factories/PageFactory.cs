using System.Diagnostics;
using PDFParser.Parser.Document;
using PDFParser.Parser.Encryption;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Factories;

public static class PageFactory
{
    public static Page Create(DictionaryObject pageDictionary, ObjectTable objects, int pageNumber, EncryptionHandler? encryptionHandler = null)
    {
        var mediaBoxArr = pageDictionary.GetAs<ArrayObject<DirectObject>>("MediaBox");
        var arguments = mediaBoxArr.Objects.OfType<NumericObject>().Select(x => (int)x.Value).ToArray();
        var mediaBox = new PageBox(arguments);

        var contents = pageDictionary["Contents"] ?? throw new UnreachableException();
        var streams = new Dictionary<IndirectReference, StreamObject>();

        switch (contents)
        {
            case ReferenceObject contentReference:
            {
                AddStreamByReference(contentReference);
                break;
            }
            case ArrayObject<DirectObject> contentArray:
            {
                foreach (var contentRef in contentArray.Objects.OfType<ReferenceObject>())
                {
                    AddStreamByReference(contentRef);
                }
                break;
            }
        }

        var resources = pageDictionary["Resources"] ?? throw new UnreachableException();

        var resourceDictionary = resources switch
        {
            DictionaryObject dict => dict,
            ReferenceObject referenceObject => objects[referenceObject.Reference] as DictionaryObject,
            _ => throw new ArgumentOutOfRangeException()
        } ?? throw new UnreachableException();
        
        var fontDictionary = resourceDictionary.GetAs<DictionaryObject>("Font").Dictionary
            .Aggregate(
                new Dictionary<string, Font>(),
                (dict, kvp) =>
                {
                    var reference = kvp.Value as ReferenceObject ?? throw new UnreachableException();
                    var fontObject = objects.GetAs<DictionaryObject>(reference.Reference) ?? throw new UnreachableException();
                    dict[kvp.Key.Name] = FontFactory.Create(fontObject, objects);
                    return dict;
                }) ?? throw new UnreachableException();   
        
        //Annotations
        // var annotations = pageDictionary.GetAs<DirectObject>("Annots");
        // switch (annotations)
        // {
        //     case ArrayObject annoArray:
        //         break;
        //     case ReferenceObject annoRef:
        //         var annoObj = objects.GetAs<DirectObject>(annoRef.Reference);
        //         if (annoObj is ArrayObject annoArr)
        //         {
        //             foreach (var o in annoArr.Objects)
        //             {
        //                 var annotationObjRef = (ReferenceObject)o;
        //                 var annotationObj = objects.GetAs<DictionaryObject>(annotationObjRef.Reference);
        //                 var formField = new AcroFormFieldDictionary(annotationObj);
        //                 var value = formField.FieldValue;
        //             }
        //         }
        //         break;
        // }
        
        
        return new Page(mediaBox, streams.AsReadOnly(), fontDictionary, pageNumber, encryptionHandler);

        void AddStreamByReference(ReferenceObject reference)
        {
            
            var contentObj = objects.GetAs<DirectObject>(reference.Reference);
            switch (contentObj)
            {
                case StreamObject contentStream:
                    streams.Add(reference.Reference, contentStream);
                    return;
                case ArrayObject<DirectObject> contentArray:
                    foreach (var arrObj in contentArray.Objects)
                    {
                        if (arrObj is ReferenceObject refObj)
                        {
                            AddStreamByReference(refObj);
                        }
                    }
                    return;
            }
        }
    }
}