// See https://aka.ms/new-console-template for more information

using PDFParser.Parser;

var fileBytes = File.ReadAllBytes(args[0]);

var parser = new PdfParser(fileBytes);

parser.Parse();
