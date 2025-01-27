using System.Text;
using BenchmarkDotNet.Attributes;
using PDFParser.Parser;

namespace PDFParser.Benchmark;

[MemoryDiagnoser]
public class DefaultBenchmark
{
    private const string A4 = "TestPDFs/A4.pdf";
    private const string Statement = "TestPDFs/Statement.pdf";
    
    private PdfParser _parserA4 = null!;
    private PdfParser _parserStatement = null!;
    
    
    [GlobalSetup]
    public void SetDefaultBenchmark()
    {
        _parserA4 = new PdfParser(File.ReadAllBytes(A4));
        _parserStatement = new PdfParser(File.ReadAllBytes(Statement));
    }

    // [Benchmark]
    // public void ParseA4()
    // {
    //     var doc= _parserA4.Parse();
    //     var pages = doc.Pages;
    //     var textPageOne = pages[0].Texts;
    // }

    [Benchmark]
    public void ParseStatement()
    {
        var doc = _parserStatement.Parse();
        //
        var pages = doc.Pages;
        var textPageOne = pages[0].Texts;
    }
}