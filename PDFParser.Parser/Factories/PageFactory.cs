using System.Diagnostics;
using ImpromptuInterface;
using PDFParser.Parser.Document;
using PDFParser.Parser.Document.Annotations;
using PDFParser.Parser.Document.Forms;
using PDFParser.Parser.Document.Structure;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Factories;

public static class PageFactory
{
    public static Page Create(DictionaryObject pageDictionary, ObjectTable objects)
    {
        var mediaBoxArr = pageDictionary.GetAs<ArrayObject<DirectObject>>("MediaBox");
        var arguments = mediaBoxArr.Objects.OfType<NumericObject>().Select(x => (int)x.Value).ToArray();
        var mediaBox = new PageBox(arguments);

        var contents = pageDictionary["Contents"] ?? throw new UnreachableException();
        var streams = new List<StreamObject>();

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
        
        
        return new Page(mediaBox, streams, fontDictionary);

        void AddStreamByReference(ReferenceObject reference)
        {
            var contentStream = objects.GetAs<StreamObject>(reference.Reference);
            streams.Add(contentStream);
        }
    }
}