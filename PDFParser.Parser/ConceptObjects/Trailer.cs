using PDFParser.Parser.Objects;

namespace PDFParser.Parser.ConceptObjects;

public class Trailer
{
    private readonly DictionaryObject _dict;
    
    public Trailer(DictionaryObject trailerDictionary)
    {
        _dict = trailerDictionary;
    }

    public ReferenceObject Root => _dict.GetAs<ReferenceObject>("Root");
    public ReferenceObject? Encrypt => _dict.TryGetAs<ReferenceObject>("Encrypt");
    public double Size => _dict.GetAs<NumericObject>("Size").Value;
    public double? Prev => _dict.TryGetAs<NumericObject>("Prev")?.Value;
    
    //An array of two byte-strings constituting a PDF file identifier (File Identifier) for the pdf. 
    //Each PDF file identifier shall have a minimum length of 16 bytes. 
    //Note: Because the ID entries are not encrypted, the ID key can be checked to assure that the correct PDF file is being accessed without decrypting the PDF file. The restrictions that the objects all be direct objects and not be encrypted ensure this.
    public ArrayObject<DirectObject>? Identifiers => _dict.TryGetAs<ArrayObject<DirectObject>>("ID");
}