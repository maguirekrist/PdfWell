using PDFParser.Parser.IO;

namespace PDFParser.Tests;

public class MatcherTests
{

    [Test]
    public void KmpTest()
    {

        var stream = "there are a lot of testcases in the code we've developed.".Select(x => (byte)x).ToArray();
        var pattern = "testcase"u8;

        var firstOffset = new KMPByteMatcher().FindFirstOffset(stream, pattern);
        Assert.That(firstOffset, Is.EqualTo(19));
    }

    [Test]
    public void BoyerMoore()
    {
        var stream = "there are a lot of testcases in the code we've developed.".Select(x => (byte)x).ToArray();
        var pattern = "testcase"u8;

        var firstOffset = new BoyerMooreMatcher().FindFirstOffset(stream, pattern);
        Assert.That(firstOffset, Is.EqualTo(19));
    }
}