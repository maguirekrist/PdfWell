namespace PDFParser.Tests;

public class UnicodeTests
{
    [Test]
    public void AsciiToUtf16()
    {
        byte ascii_code = 65;
        char capital_A = 'A';
        
        Assert.That(sizeof(char), Is.EqualTo(2));
        Assert.That(sizeof(byte), Is.EqualTo(1));
        
        Assert.That((int)capital_A, Is.EqualTo((int)ascii_code));

        
        
    }
}