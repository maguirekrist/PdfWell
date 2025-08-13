using PDFParser.Parser;
using PDFParser.Parser.Graphics;
using PDFParser.Parser.IO;
using PDFParser.Parser.Objects;

namespace PDFParser.Tests;

public static class PdfCases
{
    public static IEnumerable<string> Files()
    {
        var root = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestPdfs");
        if (!Directory.Exists(root))
            yield break;

        // Deterministic order helps with debugging
        foreach (var p in Directory.EnumerateFiles(root, "*.pdf", SearchOption.AllDirectories).OrderBy(x => x))
            yield return p;
    }
}

public class Tests
{
    private const string A4 = "TestPDFs/A4.pdf";
    private const string TwoPager = "TestPDFs/two_pager.pdf";
    private const string MultiText = "TestPDFs/multi_text.pdf";
    private const string G1145 = "TestPDFs/g-1145.pdf";
    private const string Resume = "TestPDFs/Resume.pdf";
    private const string Ssa89 = "TestPDFs/ssa-89.pdf";
    private const string SimpleForm = "TestPDFs/test_me.pdf";
    private const string I130 = "TestPDFs/i-130.pdf";
    
    
    
    [SetUp]
    public void Setup()
    {
    }
    
    [TestCaseSource(typeof(PdfCases), nameof(PdfCases.Files))]
    public void ParsePdf_should_not_throw(string pdfPath)
    {
        Assert.That(File.Exists(pdfPath), Is.True);
        var pdfData = File.ReadAllBytes(pdfPath);
        var parser = new PdfParser(pdfData);
        var document = parser.Parse();
        
        Assert.That(document, Is.Not.Null);
        Assert.That(document.Pages, Is.Not.Null);
        Assert.That(document.Pages, Is.Not.Empty);
    }

    [Test]
    public void TestStrangePdf()
    {
        Assert.That(File.Exists("TestPDFs/fw9.pdf"), Is.True);
        var pdfData = File.ReadAllBytes("TestPDFs/fw9.pdf");
        var parser = new PdfParser(pdfData);
        var document = parser.Parse();
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
    public void TestResume()
    {
        var pdfData = File.ReadAllBytes(Resume);
        var parser = new PdfParser(pdfData);
        var document = parser.Parse();
        
        Assert.That(document.Pages.Count, Is.EqualTo(1));
        var texts = document.Pages[0].GetTexts();
        Assert.NotNull(texts);
        Assert.True(texts.Count > 0);

        document.Save("test_resume.pdf");
    }

    [Test]
    public void TestSimpleAcroForm()
    {
        var pdfData = File.ReadAllBytes(SimpleForm);
        var parser = new PdfParser(pdfData);
        var document = parser.Parse();
        Assert.That(document.Pages.Count, Is.EqualTo(1));
        var form = document.GetAcroForm();
        Assert.That(form, Is.Not.Null);

        var fields = form.GetFields();
        Assert.That(fields, Is.Not.Empty);
        Assert.That(fields.Count, Is.EqualTo(1));

        document.Save("simple_form.pdf");
    }

    [Test]
    public void TestSimpleGovForm()
    {
        var pdfData = File.ReadAllBytes(Ssa89);
        var parser = new PdfParser(pdfData);
        var document = parser.Parse();
        
        Assert.That(document.Pages.Count, Is.EqualTo(1));
        var texts = document.Pages[0].GetTexts();

        Assert.That(texts, Is.Not.Empty);
        
        var acroForm = document.GetAcroForm();
        Assert.That(acroForm, Is.Not.Null);
        var fields = acroForm.GetFields();
        Assert.That(fields, Is.Not.Empty);


        List<Rectangle> rects = new();
        foreach (var field in fields)
        {
            var fieldWidget = field.Widget;
            if (fieldWidget != null)
            {
                var rectArray = fieldWidget.Rect;
                rects.Add(Rectangle.FromPdfArray(rectArray));
                
                var page = fieldWidget.Page as ReferenceObject;
                var pageObj = document.GetPage(page!.Reference);
                
                Assert.That(pageObj, Is.Not.Null);
            }
        }
        
        Assert.That(rects, Is.Not.Empty);
        
        document.Save("test_gov2.pdf");
    }

    [Test]
    public void TestI130()
    {
        var pdfData = File.ReadAllBytes(I130);
        var parser = new PdfParser(pdfData);
        var document = parser.Parse();
        
        Assert.That(document.Pages.Count, Is.Not.EqualTo(0));
        //var texts = document.Pages[0].GetTexts();

        //Assert.That(texts, Is.Not.Empty);
        
        var acroForm = document.GetAcroForm();
        Assert.That(acroForm, Is.Not.Null);
        var fields = acroForm.GetFields();
        Assert.That(fields, Is.Not.Empty);


        List<Rectangle> rects = new();
        foreach (var field in fields)
        {
            var fieldWidget = field.Widget;
            if (fieldWidget != null)
            {
                var rectArray = fieldWidget.Rect;
                rects.Add(Rectangle.FromPdfArray(rectArray));
                
                var page = fieldWidget.Page as ReferenceObject;
                var pageObj = document.GetPage(page!.Reference);
                
                Assert.That(pageObj, Is.Not.Null);
            }
        }
        
        Assert.That(rects, Is.Not.Empty);
        
        document.Save("test_gov3.pdf");
    }
    
}