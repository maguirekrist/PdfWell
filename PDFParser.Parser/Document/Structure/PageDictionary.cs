using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Document.Structure;

public class PageDictionary
{
    private readonly DictionaryObject _dict;

    public PageDictionary(DictionaryObject dict)
    {
        //TODO: Refactor type checking into a separate factory method pattern.
        if (dict.GetAs<NameObject>("Type") is { Name: not "Page" } typeObj)
        {
            throw new Exception($"Attempt to instantiate a {nameof(PageTreeDictionary)} object from a dictionary of type: {typeObj.Name}");
        }
        
        _dict = dict;
    }
    
    
}