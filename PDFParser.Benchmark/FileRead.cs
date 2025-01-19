using System.Buffers;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace PDFParser.Benchmark;

[MemoryDiagnoser]
public class FileRead
{
    private const string PdfPath = "TestPDFs/BigPdf.pdf";
    private long FileLength;
    
    [GlobalSetup]
    public void Setup()
    {
        var fileInfo = new FileInfo(PdfPath);
        FileLength = fileInfo.Length;
    }
    
    [Benchmark(Baseline = true)]
    public void ReadAllBytes()
    {
        var bytes = File.ReadAllBytes(PdfPath);
        Debug.Assert(bytes.Length == FileLength);
    }

    [Benchmark]
    public void ReadFileStream()
    {
        using FileStream fs = new FileStream(PdfPath, FileMode.Open, FileAccess.Read);
        {
            var fileSize = fs.Length;
            fs.Seek(0, SeekOrigin.Begin);
            var data = new byte[fileSize];
            var read = fs.Read(data, 0, (int)fileSize);
            Debug.Assert(data.Length == FileLength);
        }
    }

    [Benchmark]
    public void ReadMemoryMappedFile()
    {
        using MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(PdfPath, FileMode.Open);
        {
            using var accessor = mmf.CreateViewAccessor();
            unsafe
            {
                byte* ptr = null;
                accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                try
                {
                    
                    var memory = new Span<byte>(ptr, (int)accessor.Capacity);
                }
                finally
                {
                    accessor.SafeMemoryMappedViewHandle.ReleasePointer();
                }
            }
        } 
    }
}