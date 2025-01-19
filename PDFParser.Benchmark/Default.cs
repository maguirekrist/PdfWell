using System.Text;
using BenchmarkDotNet.Attributes;
using PDFParser.Parser;

namespace PDFParser.Benchmark;

[MemoryDiagnoser]
public class DefaultBenchmark
{
    private const string PdfPath = "TestPDFs/A4.pdf";

    private byte[] _fileData = null!;
    private PdfParser _parser = null!;
    
    [GlobalSetup]
    public void SetDefaultBenchmark()
    {
        _fileData = File.ReadAllBytes(PdfPath);
        _parser = new PdfParser(_fileData);
    }

    [Benchmark]
    public void ParsePdf()
    {
        _parser.Parse();
    }
}