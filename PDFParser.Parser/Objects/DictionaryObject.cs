namespace PDFParser.Parser.Objects;

public class DictionaryObject : DirectObject
{
    private Dictionary<NameObject, DirectObject> _dictionary;

    public Dictionary<NameObject, DirectObject> Dictionary => _dictionary;

    public bool IsStream => _dictionary.ContainsKey(new NameObject("Length"));
    
    public StreamObject? Stream { get; set; }
    
    public DictionaryObject(Dictionary<NameObject, DirectObject> dictionary, long offset, long length) : base(offset, length)
    {
        _dictionary = dictionary;
    }
    
    public DirectObject? this[string key]
    {
        get => _dictionary[new NameObject(key)];
        set => _dictionary[new NameObject(key)] = value ?? throw new ArgumentNullException();
    }
    
}