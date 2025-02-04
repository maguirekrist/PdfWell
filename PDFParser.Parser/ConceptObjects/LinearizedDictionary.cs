using PDFParser.Parser.Attributes;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.ConceptObjects;

public class LinearizedDictionary
{
    [PdfDictionaryKey("L")]
    public required NumericObject LengthOfFile { get; init; }
    [PdfDictionaryKey("H")]
    public required ArrayObject HintOffsets { get; init; }
    [PdfDictionaryKey("O")]
    public required NumericObject FirstPageObjectNumber { get; init; }
    [PdfDictionaryKey("E")]
    public required NumericObject OffsetToEndOfFirstPage { get; init; }
    [PdfDictionaryKey("N")]
    public required NumericObject PagesCount { get; init; }
    [PdfDictionaryKey("T")]
    public required NumericObject MainCrossReferenceOffset { get; init; }
    [PdfDictionaryKey("P")]
    public NumericObject? PageNumberOfFirstPage { get; init; }
}