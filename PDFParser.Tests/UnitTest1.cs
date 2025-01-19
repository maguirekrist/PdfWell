using PDFParser.Parser;

namespace PDFParser.Tests;

public class Tests
{
    private string _pdfPath;
    private PdfParser _parser;
    
    [SetUp]
    public void Setup()
    {
        _pdfPath = "TestPDFs/A4.pdf";
        var pdfData = File.ReadAllBytes(_pdfPath);
        _parser = new PdfParser(pdfData);
    }

    [Test]
    public void CanFindXrefOffsetStart()
    {
        _parser.Parse();
        // PdfParser parser = new PdfParser();
        // parser.ParsePdf2(_pdfPath);
        // Console.WriteLine(parser.GetPdfVersion());
        // Assert.That(parser.GetPdfVersion(), Is.EqualTo("%PDF-1.7"));
        // Assert.That(parser.GetXrefOffsetStart(), Is.EqualTo(478));
        // Assert.That(parser.XrefTable.Count, Is.EqualTo(6));
    }
    
    
}