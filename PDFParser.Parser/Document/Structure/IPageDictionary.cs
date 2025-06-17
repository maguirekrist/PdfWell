using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Document.Structure;

public interface IPageDictionary
{
    NameObject Type { get; }
    ReferenceObject Parent { get; }
    DictionaryObject Resources { get; }
    ArrayObject<NumericObject> ? MediaBox { get; }
    ArrayObject<NumericObject> ? CropBox { get; }
    ArrayObject<DirectObject> ? Annots { get; }
}