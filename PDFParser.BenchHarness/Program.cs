using System.Diagnostics;
using System.Text;
using PDFParser.Parser;

// args: [corpusDir] [repeats]
var solutionRoot = FindSolutionRoot();
var corpusDir = args.Length > 0 ? args[0] : Path.Combine(solutionRoot, "TestPDFs");
var repeats   = args.Length > 1 ? int.Parse(args[1]) : 1;

var files = Directory.EnumerateFiles(corpusDir, "*.pdf", SearchOption.AllDirectories).ToArray();
if (files.Length == 0) { Console.Error.WriteLine($"No PDFs in {corpusDir}"); return; }

// Warmup
_ = GC.GetTotalMemory(true);

var elapsed = new List<double>(files.Length * repeats);
long peakWorkingSet = 0;

var outDir = Path.Combine(AppContext.BaseDirectory, "out");
Directory.CreateDirectory(outDir);
var csvPath = Path.Combine(outDir, $"results-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");
using var csv = new StreamWriter(csvPath);
csv.WriteLine("file,size_bytes,elapsed_ms,alloc_bytes,working_set_mb");

foreach (var _ in Enumerable.Range(0, repeats))
{
    foreach (var f in files)
    {
        try
        {
            var bytes = File.ReadAllBytes(f); // keep disk I/O outside parse if you can
            var p = Process.GetCurrentProcess();
            var beforeAlloc = GC.GetAllocatedBytesForCurrentThread();

            var sw = Stopwatch.StartNew();
            ParsePdf(bytes);                   // <<< call your library here
            sw.Stop();

            var afterAlloc = GC.GetAllocatedBytesForCurrentThread();
            var ws = p.WorkingSet64;          // current RSS
            if (ws > peakWorkingSet) peakWorkingSet = ws;

            elapsed.Add(sw.Elapsed.TotalMilliseconds);
            csv.WriteLine($"{Path.GetFileName(f)},{bytes.Length},{sw.Elapsed.TotalMilliseconds:F3},{afterAlloc - beforeAlloc},{ws / 1024.0 / 1024.0:F1}");
        }
        catch (Exception e)
        {
            csv.WriteLine($"{Path.GetFileName(f)},n/a,n/a,n/a,n/a");
        }
    }
}

elapsed.Sort();
double Q(double q)
{
    if (elapsed.Count == 0) return 0;
    var i = q * (elapsed.Count - 1);
    var lo = (int)Math.Floor(i); var hi = (int)Math.Ceiling(i);
    return lo == hi ? elapsed[lo] : elapsed[lo] + (elapsed[hi] - elapsed[lo]) * (i - lo);
}

static void ParsePdf(byte[] bytes)
{
    var parser = new PdfParser(bytes);
    var document = parser.Parse();
    var catalog = document.DocumentCatalog;
    var pages = document.Pages;
    var pageCount = pages.Count;
    var form = document.GetAcroForm();
    if (form != null)
    {
        var fields = form.GetFields();
    }
}

static string FindSolutionRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null && !dir.EnumerateFiles("*.sln").Any())
        dir = dir.Parent;

    if (dir == null)
        throw new InvalidOperationException("No .sln file found in parent directories.");

    return dir.FullName;
}

Console.WriteLine($@"Count={elapsed.Count}  p50={Q(0.50):F2} ms  p95={Q(0.95):F2} ms  PeakRSS={(peakWorkingSet/1024.0/1024.0):F1} MB");
Console.WriteLine($@"CSV: {csvPath}");
