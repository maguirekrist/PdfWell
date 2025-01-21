// See https://aka.ms/new-console-template for more information

using PDFParser.Parser;

var fileBytes = File.ReadAllBytes(args[0]);

var parser = new PdfParser(fileBytes);

var document = parser.Parse();

Console.WriteLine(document.Pages.Count);

var page = document.GetPage(1);

var texts = page.GetTexts();

//win!