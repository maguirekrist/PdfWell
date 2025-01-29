using System.Text;
using BenchmarkDotNet.Attributes;
using PDFParser.Parser;
using PDFParser.Parser.IO;

namespace PDFParser.Benchmark;

[MemoryDiagnoser]
public class DefaultBenchmark
{
    private const string A4 = "TestPDFs/A4.pdf";
    private const string Statement = "TestPDFs/Statement.pdf";

    private byte[] _a4Bytes = [];
    private byte[] _statementBytes = [];
    
    
    [GlobalSetup]
    public void SetDefaultBenchmark()
    {
        _a4Bytes = File.ReadAllBytes(A4);
        _statementBytes = File.ReadAllBytes(Statement);
    }

    [Benchmark]
    public object ParseA4()
    {
        var parser = new PdfParser(_a4Bytes);
        var doc = parser.Parse();
        return doc ?? new object();
        // var pages = doc.Pages;
        // var textPageOne = pages[0].Texts;
    }

    [Benchmark]
    public object ParseStatement()
    {
        var parser = new PdfParser(_statementBytes);
        var doc = parser.Parse();
        return doc;
        // //
        // var pages = doc.Pages;
        // var textPageOne = pages[0].Texts;
    }
    
    [Benchmark]
    public object ParseStatementKmp()
    {
        var parser = new PdfParser(_statementBytes, new KMPByteMatcher());
        var doc = parser.Parse();
        return doc;
        // //
        // var pages = doc.Pages;
        // var textPageOne = pages[0].Texts;
    }
    
    [Benchmark]
    public object ParseStatementSunday()
    {
        var parser = new PdfParser(_statementBytes, new BoyerMooreMatcher());
        var doc = parser.Parse();
        return doc;
        // //
        // var pages = doc.Pages;
        // var textPageOne = pages[0].Texts;
    }
}