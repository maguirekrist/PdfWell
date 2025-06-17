using PDFParser.Parser.Objects;

namespace PDFParser.Parser.ConceptObjects;


//Handles parsing of Hint Stream and handling a Hint Dictionary. 
public class HintTables
{
    private readonly DictionaryObject _dict;

    public HintTables(DictionaryObject dict)
    {
        _dict = dict;
    }

    private int SharedObjectOffset => (int)_dict.GetAs<NumericObject>("S").Value;
    
}