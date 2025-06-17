using PDFParser.Parser.Attributes;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.ConceptObjects;

//This is at test class for a different approach for Abstract PDF Objects...


public record FormFieldDictionary
{
    public required NameObject Type { get; init; }
    public DictionaryObject? Parent { get; init; }
    public ArrayObject<DirectObject>? Kids { get; init; }
    [PdfDictionaryKey("T")]
    public StringObject? FieldName { get; init; }
    [PdfDictionaryKey("TU")]
    public StringObject? AlternativeDescription { get; init; }
    [PdfDictionaryKey("TM")]
    public StringObject? MappingName { get; init; }
    [PdfDictionaryKey("Ff")]
    public NumericObject? SetFlags { get; init; }
    [PdfDictionaryKey("V")]
    public DirectObject? Value { get; init; }
    [PdfDictionaryKey("DV")]
    public DirectObject? DefaultValue { get; init; }
    [PdfDictionaryKey("AA")]
    public DictionaryObject? TriggerEvents { get; init; }
}