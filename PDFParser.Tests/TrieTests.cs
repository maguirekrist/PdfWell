using PDFParser.Parser.Utils;

namespace PDFParser.Tests;

public class TrieTests
{

    [Test]
    public void TrieBasics()
    {
        var trieTest = new Trie();

         trieTest.Insert("Test Me");
         trieTest.Insert("Test You");
        
         var results = trieTest.Search("Test");
         Assert.That(results.Count, Is.EqualTo(2));
        
         Assert.That(trieTest.Contains("Test Me"), Is.True);
        Assert.That(trieTest.Contains("Not In"), Is.False);
        
        trieTest.Insert("carpet");
        trieTest.Insert("car");
        trieTest.Insert("carpool");

        var carResults = trieTest.Search("car");
        Assert.That(carResults.Contains("car"), Is.True);
        Assert.That(carResults.Contains("carpet"), Is.True);
        
        Assert.That(trieTest.Contains("car"), Is.True);
        Assert.That(trieTest.Contains("carpet"), Is.True);
    }

    [Test]
    public void CompressedTrieBasics()
    {
        var trieTest = new CompressedTrie();

        // trieTest.Insert("Test Me");
        // trieTest.Insert("Test You");
        //
        // var results = trieTest.Search("Test");
        // Assert.That(results.Count, Is.EqualTo(2));
        //
        // Assert.That(trieTest.Contains("Test Me"), Is.True);
        // Assert.That(trieTest.Contains("Not In"), Is.False);
        
        trieTest.Insert("test");
        trieTest.Insert("water");
        trieTest.Insert("slow");

        // var carResults = trieTest.Search("car");
        // Assert.That(carResults.Contains("car"), Is.True);
        // Assert.That(carResults.Contains("carpet"), Is.True);
        
        Assert.That(trieTest.Contains("test"), Is.True);
        Assert.That(trieTest.Contains("water"), Is.True);
        Assert.That(trieTest.Contains("slow"), Is.True);

        trieTest.Insert("slower");
        Assert.That(trieTest.Contains("slow"));
        Assert.That(trieTest.Contains("slower"));
        
        trieTest.Insert("tester");
        Assert.That(trieTest.Contains("test"));
        Assert.That(trieTest.Contains("tester"));

        trieTest.Insert("team");
        Assert.That(trieTest.Contains("test"));
        Assert.That(trieTest.Contains("team"));
        Assert.That(trieTest.Contains("tester"));

        var testResults = trieTest.Search("te");
        Assert.That(testResults.Count, Is.EqualTo(3));
        
        trieTest.Clear();
        
        trieTest.Insert("test");
        trieTest.Insert("tester"); 
        trieTest.Insert("team");
        trieTest.Insert("tear");
        testResults = trieTest.Search("te");
        Assert.That(testResults.Contains("test"));
        Assert.That(testResults.Contains("tester"));
        Assert.That(testResults.Contains("team"));
        Assert.That(testResults.Contains("tear"));
        
        testResults = trieTest.Search("tes");
        Assert.That(testResults.Contains("test"));
        Assert.That(testResults.Contains("tester"));

        testResults = trieTest.Search("qart");
        Assert.That(testResults.Count, Is.EqualTo(0));
        
        trieTest.Insert("team");
        Assert.That(trieTest.Contains("test"));
        Assert.That(trieTest.Contains("tester"));
        Assert.That(trieTest.Contains("team"));
        Assert.That(trieTest.Contains("tear"));
        Assert.That(trieTest.Contains("tes"), Is.False);
        
        
        trieTest.Insert("te");
        trieTest.Insert("team");
        trieTest.Insert("te");
        
        Assert.That(trieTest.Contains("te"));
        Assert.That(trieTest.Contains("team"));

        testResults = trieTest.Search("te");
        Assert.That(testResults.Contains("te"));

        testResults = trieTest.Search("teb");
        Assert.That(testResults.Any(), Is.False);

        trieTest.Clear();
        
        trieTest.Insert("team");
        testResults = trieTest.Search("teb");
        Assert.That(testResults.Count, Is.EqualTo(0));

    }
}