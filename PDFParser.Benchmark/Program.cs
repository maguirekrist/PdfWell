// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using PDFParser.Benchmark;

var summary1 = BenchmarkRunner.Run<DefaultBenchmark>();
var summary2 = BenchmarkRunner.Run<ParserMemoryBenchmark>();
