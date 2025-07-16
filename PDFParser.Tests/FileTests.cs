using PDFParser.Parser;
using PDFParser.Parser.IO;

namespace PDFParser.Tests;

public class Tests
{
    private const string A4 = "TestPDFs/A4.pdf";
    private const string TwoPager = "TestPDFs/two_pager.pdf";
    private const string MultiText = "TestPDFs/multi_text.pdf";
    private const string G1145 = "TestPDFs/g-1145.pdf";
    private const string BankStatement = "TestPDFs/Statement.pdf";
    private const string Resume = "TestPDFs/Resume.pdf";
    private const string SSA89 = "TestPDFs/SSA-89.pdf";
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void XrefMatcherText()
    {
        var pdfData = File.ReadAllBytes(A4);
        var parser = new PdfParser(pdfData);
        var defaultXrefOffset = parser.FindStartXrefOffset();

        var newParser = new PdfParser(pdfData, new BoyerMooreMatcher());
        var xrefOffset = newParser.FindStartXrefOffset();
        
        Assert.That(xrefOffset, Is.EqualTo(defaultXrefOffset));
    }

    [Test]
    public void TestA4()
    {
        var pdfData = File.ReadAllBytes(A4);
        var parser = new PdfParser(pdfData, new BoyerMooreMatcher());
        var document = parser.Parse(); 
        
        Assert.That(document.ObjectTable.Count, Is.EqualTo(6));
        Assert.That(document.Pages.Count, Is.EqualTo(1));
        Assert.That(document.Pages[0].GetTexts()[0].Value, Is.EqualTo("this is a test."));
        // PdfParser parser = new PdfParser();
        // parser.ParsePdf2(_pdfPath);
        // Console.WriteLine(parser.GetPdfVersion());
        // Assert.That(parser.GetPdfVersion(), Is.EqualTo("%PDF-1.7"));
        // Assert.That(parser.GetXrefOffsetStart(), Is.EqualTo(478));
        // Assert.That(parser.XrefTable.Count, Is.EqualTo(6));

        document.Save("test.pdf");
    }

    [Test]
    public void TestTwoPager()
    {
        var pdfData = File.ReadAllBytes(TwoPager);
        var parser = new PdfParser(pdfData);
        var document = parser.Parse();
        
        //Assert.That(document.Objects.Count, Is.EqualTo(6));
        Assert.That(document.Pages.Count, Is.EqualTo(2));
        Assert.That(document.Pages[0].GetTexts()[0].Value, Is.EqualTo("this is a test."));
        Assert.That(document.Pages[1].GetTexts()[0].Value, Is.EqualTo("I'm on the second page"));

        document.Save("test_two.pdf");
    }

    [Test]
    public void TestMultiText()
    {
        var pdfData = File.ReadAllBytes(MultiText);
        var parser = new PdfParser(pdfData);
        var document = parser.Parse();
        
        Assert.That(document.Pages.Count, Is.EqualTo(1));

        document.Save("test_three.pdf");
    }

    [Test]
    public void TestGovernmentPdf()
    {
        var pdfData = File.ReadAllBytes(G1145);
        var parser = new PdfParser(pdfData);
        var document = parser.Parse();
        
        Assert.That(document.Pages.Count, Is.EqualTo(1));

        var acroDictionary = document.GetAcroForm();
        Assert.That(acroDictionary, Is.Not.Null);

        document.Save("test_gov.pdf");
    }

    [Test]
    public void TestBankStatement()
    {
        var pdfData = File.ReadAllBytes(BankStatement);
        var parser = new PdfParser(pdfData);
        var document = parser.Parse();
        
        Assert.That(document.Pages.Count, Is.EqualTo(2));
        foreach (var text in document.Pages[0].GetTexts())
        {
            Console.WriteLine(text);
        }

        document.Save("test_bank.pdf");
    }

    [Test]
    public void TestResume()
    {
        var pdfData = File.ReadAllBytes(Resume);
        var parser = new PdfParser(pdfData);
        var document = parser.Parse();
        
        Assert.That(document.Pages.Count, Is.EqualTo(1));
        var texts = document.Pages[0].GetTexts();
        foreach (var text in texts)
        {
            Console.WriteLine(text);
        }
        Assert.NotNull(texts);
        Assert.True(texts.Count > 0);

        document.Save("test_resume.pdf");
    }

    [Test]
    public void TestSimpleGovForm()
    {
        var pdfData = File.ReadAllBytes(SSA89);
        var parser = new PdfParser(pdfData);
        var document = parser.Parse();
        
        Assert.That(document.Pages.Count, Is.EqualTo(1));
        var texts = document.Pages[0].GetTexts();

        foreach (var text in texts)
        {
            Console.WriteLine(text);
        }

        var acroForm = document.GetAcroForm();
        Assert.That(acroForm, Is.Not.Null);
        var fields = acroForm.GetFields();
        Assert.That(fields, Is.Not.Empty);

        document.Save("test_gov2.pdf");
    }
    
}