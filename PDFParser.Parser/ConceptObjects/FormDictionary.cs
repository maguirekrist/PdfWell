using PDFParser.Parser.Attributes;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.ConceptObjects;

public class FormDictionary
{
    public required ArrayObject Fields { get; init; }
    public BooleanObject? NeedAppearances { get; init; }
    public NumericObject? SigFlags { get; init; }
    [PdfDictionaryKey("CO")]
    public ArrayObject? CalculationOrder { get; init; }
    [PdfDictionaryKey("DR")]
    public DictionaryObject? ResourceDictionary { get; init; }
    [PdfDictionaryKey("DA")]
    public StringObject? DefaultTextValue { get; init; }
    [PdfDictionaryKey("Q")]
    public NumericObject? DefaultNumericValue { get; init; }
    [PdfDictionaryKey("XFA")]
    public DirectObject? XfaResource { get; init; }
}