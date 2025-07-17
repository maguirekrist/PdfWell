using PDFParser.Parser.IO;
using PDFParser.Parser.Lexer;

namespace PDFParser.Tests;

public class LexerTests
{

    private const string A4 = "TestPDFs/A4.pdf";
    private const string SSA89 = "TestPDFs/ssa-89.pdf";
    
    [Test]
    public void ShouldTokenize()
    {
        var pdfData = File.ReadAllBytes(A4);
        var reader = new MemoryInputBytes(pdfData);
        var scanner = new PdfTokenizer(reader);
        var tokens = scanner.ScanTokens();
        
        Assert.That(tokens, Is.Not.Empty);
    }

    [Test]
    public void ShouldTokenizeComplexPdf()
    {
        var pdfData = File.ReadAllBytes(SSA89);
        var reader = new MemoryInputBytes(pdfData);
        var scanner = new PdfTokenizer(reader);
        var tokens = scanner.ScanTokens();
        
        Assert.That(tokens, Is.Not.Empty);
    }
}