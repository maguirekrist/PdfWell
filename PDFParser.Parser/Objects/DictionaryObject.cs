using System.Dynamic;

namespace PDFParser.Parser.Objects;

public class DictionaryObject : DirectObject
{
    private readonly Dictionary<NameObject, DirectObject> _dictionary;

    public Dictionary<NameObject, DirectObject> Dictionary => _dictionary;

    public bool HasLength => _dictionary.ContainsKey(new NameObject("Length"));

    public NameObject? Type => TryGetAs<NameObject>("Type");
    
    // public StreamObject? Stream { get; set; }

    public DictionaryObject(Dictionary<NameObject, DirectObject> dictionary) : base(0, 0)
    {
        _dictionary = dictionary;
    }
    
    public DictionaryObject(Dictionary<NameObject, DirectObject> dictionary, long offset, int length) : base(offset, length)
    {
        _dictionary = dictionary;
    }

    public DictionaryObject(DictionaryObject dictionaryObject) : base(dictionaryObject.Offset, dictionaryObject.Length)
    {
        _dictionary = dictionaryObject.Dictionary;
    }
    
    public DirectObject? this[string key]
    {
        get => _dictionary.GetValueOrDefault(new NameObject(key));
        set => _dictionary[new NameObject(key)] = value ?? throw new ArgumentNullException();
    }

    public T GetAs<T>(string key) where T : DirectObject
    {
        var namedKey = new NameObject(key);
        if (!_dictionary.TryGetValue(namedKey, out var value))
        {
            throw new KeyNotFoundException($"Key '{key}' was not found in the dictionary.");
        }

        if (value is T result)
        {
            return result;
        }

        throw new InvalidCastException($"The value for key '{key}' is not of type {typeof(T).Name}.");
    }

    public T? TryGetAs<T>(string key) where T : DirectObject
    {
        var namedKey = new NameObject(key);
        if (!_dictionary.TryGetValue(namedKey, out var value))
        {
            return null;
        }

        if (value is T result)
        {
            return result;
        }

        return null;
    }

    public bool TryRemove(string key)
    {
        var namedKey = new NameObject(key);
        if (_dictionary.ContainsKey(namedKey))
        {
            _dictionary.Remove(namedKey);
            return true;
        }

        return false;
    }

    public bool HasKey(string key)
    {
        var namedKey = new NameObject(key);
        return _dictionary.ContainsKey(namedKey);
    }
    
}