using PDFParser.Parser;

namespace PDFParser.Tests;

public class Tests
{
    private const string A4 = "TestPDFs/A4.pdf";
    private const string TwoPager = "TestPDFs/two_pager.pdf";
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestA4()
    {
        var pdfData = File.ReadAllBytes(A4);
        var parser = new PdfParser(pdfData);
        var document = parser.Parse();
        
        Assert.That(document.ObjectTable.Count, Is.EqualTo(6));
        Assert.That(document.Pages.Count, Is.EqualTo(1));
        // PdfParser parser = new PdfParser();
        // parser.ParsePdf2(_pdfPath);
        // Console.WriteLine(parser.GetPdfVersion());
        // Assert.That(parser.GetPdfVersion(), Is.EqualTo("%PDF-1.7"));
        // Assert.That(parser.GetXrefOffsetStart(), Is.EqualTo(478));
        // Assert.That(parser.XrefTable.Count, Is.EqualTo(6));
    }

    [Test]
    public void TestTwoPager()
    {
        var pdfData = File.ReadAllBytes(TwoPager);
        var parser = new PdfParser(pdfData);
        var document = parser.Parse();
        
        //Assert.That(document.Objects.Count, Is.EqualTo(6));
        Assert.That(document.Pages.Count, Is.EqualTo(2));
    }
    
}