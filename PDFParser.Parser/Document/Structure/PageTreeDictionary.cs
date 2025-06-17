using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Document.Structure;

public class PageTreeDictionary
{
    private readonly DictionaryObject _dict;

    public PageTreeDictionary(DictionaryObject dict)
    {
        //TODO: Refactor type checking into a separate factory method pattern.
        if (dict.GetAs<NameObject>("Type") is { Name: not "Pages" } typeObj)
        {
            throw new Exception($"Attempt to instantiate a {nameof(PageTreeDictionary)} object from a dictionary of type: {typeObj.Name}");
        }
        
        _dict = dict;
    }
    
    //Required - except root node. 
    public DictionaryObject? Parent => _dict.TryGetAs<DictionaryObject>("Parent");

    public DictionaryObject Kids => _dict.GetAs<DictionaryObject>("Kids");

    public NumericObject Count => _dict.GetAs<NumericObject>("Count");
}