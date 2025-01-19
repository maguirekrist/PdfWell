using PDFParser.Parser.Objects;

namespace PDFParser.Parser;

public class PdfDocument
{
    private List<DirectObject> _objects;

    public IReadOnlyList<DirectObject> Objects => _objects.AsReadOnly();

    public PdfDocument(List<DirectObject> objects)
    {
        _objects = objects;
    }
}