using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Document.Structure;

public interface IPageDictionary
{
    NameObject Type { get; }
    ReferenceObject Parent { get; }
    DictionaryObject Resources { get; }
    ArrayObject<DirectObject> ? MediaBox { get; }
    ArrayObject<DirectObject> ? CropBox { get; }
    ArrayObject<DirectObject> ? Annots { get; }
}