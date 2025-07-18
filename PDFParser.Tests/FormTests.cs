using PDFParser.Parser;
using PDFParser.Parser.Document.Forms;

namespace PDFParser.Tests;

public class FormTests
{
    private const string Ssa89 = "TestPDFs/ssa-89.pdf";
    private const string G1145 = "TestPDFs/g-1145.pdf";
    
    [Test]
    public void FormTestOne()
    {
        var pdfData = File.ReadAllBytes(G1145);
        var parser = new PdfParser(pdfData);
        var document = parser.Parse();
        var form = document.GetAcroForm();
        Assert.That(form, Is.Not.Null);
        var fields = form.GetFields();

        var counter = 0;
        var setList = new List<string>();
        foreach (var field in fields)
        {
            if (field.FieldType == FieldType.Text)
            {
                var valu = $"Field {counter} Value";
                field.SetValue(valu);
                setList.Add(valu);
                counter++;   
            }
        }

        var memStream = new MemoryStream();
        document.Save(memStream);

        var changedPdfBytes = memStream.ToArray();
        var newParser = new PdfParser(changedPdfBytes);
        var changedDocument = newParser.Parse();
        var changedForm = changedDocument.GetAcroForm();
        Assert.That(changedForm, Is.Not.Null);
        var changedFields = changedForm.GetFields();

        counter = 0;
        foreach (var field in changedFields)
        {
            if (field.FieldType == FieldType.Text)
            {
                var fieldValue = field.GetValueAsString();
                Assert.That(fieldValue, Is.EqualTo(setList[counter]));
                counter++;
            }
        }

        //document.Save("write_test.pdf");
    }
}