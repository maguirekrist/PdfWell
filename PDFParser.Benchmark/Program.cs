// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using PDFParser.Benchmark;

var summary = BenchmarkRunner.Run<DefaultBenchmark>();
