using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using PDFParser.Parser;

namespace PDFParser.Benchmark;

[MemoryDiagnoser]
public class ParserMemoryBenchmark
{
    [Params(
        "TestPDFs/A4.pdf",
        "TestPDFs/g-1145.pdf",
        "TestPDFs/multi_text.pdf",
        "TestPDFs/Resume.pdf",
        "TestPDFs/two_pager.pdf"
    )]
    public string PdfPath { get; set; } = string.Empty;

    private byte[] _pdfBytes = Array.Empty<byte>();

    [GlobalSetup]
    public void Setup()
    {
        _pdfBytes = File.ReadAllBytes(PdfPath);
    }

    [Benchmark]
    public void ParsePdfAndRelease()
    {
        // Step 1: Read bytes (already done in setup)
        // Step 2: Parse
        var parser = new PdfParser(_pdfBytes);
        var doc = parser.Parse();
        // Optionally, touch some properties to ensure full realization
        var pageCount = doc.Pages.Count;
        // Step 3: Let doc go out of scope (happens at end of method)
        // GC will be measured by BenchmarkDotNet
    }

    [IterationCleanup]
    public void Cleanup()
    {
        // Optionally force GC to measure post-collection memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
} 