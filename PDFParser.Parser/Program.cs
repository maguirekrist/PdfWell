// See https://aka.ms/new-console-template for more information

using PDFParser.Parser;
using PDFParser.Parser.Objects;


//This is our Sandbox:


Console.WriteLine(args[0]);
var fileBytes = File.ReadAllBytes(args[0]);

var parser = new PdfParser(fileBytes);

var document = parser.Parse();

Console.WriteLine($"Total Pages: {document.Pages.Count}");

// Console.WriteLine("Cmap Stream");
// var toUnicodeStreamObject = document.GetObjectNumber<DictionaryObject>(14);
// Console.WriteLine(toUnicodeStreamObject.Stream!.DecodedStream);

var page = document.GetPage(1);

// foreach (var content in page.Contents)
// {
//     Console.WriteLine("Decoded Stream:");
//     Console.WriteLine(content.DecodedStream);
// }

var texts = page.GetTexts();

foreach (var text in texts)
{
    Console.WriteLine(text);
}

//win!