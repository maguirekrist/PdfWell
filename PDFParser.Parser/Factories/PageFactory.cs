using System.Diagnostics;
using PDFParser.Parser.Document;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Factories;

public static class PageFactory
{
    public static Page Create(DictionaryObject pageDictionary, Dictionary<IndirectReference, DirectObject> objects)
    {
        var mediaBoxArr = pageDictionary.GetAs<ArrayObject>("MediaBox");
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
            case ArrayObject contentArray:
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
            ReferenceObject referenceObject => referenceObject.Value as DictionaryObject,
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
        
        
        return new Page(mediaBox, streams, fontDictionary);

        void AddStreamByReference(ReferenceObject reference)
        {
            var contentDict = objects.GetAs<DictionaryObject>(reference.Reference);
            var stream = contentDict.Stream;

            if (stream == null)
            {
                throw new UnreachableException();
            }

            streams.Add(stream);
        }
    }
}