using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Utils;

public static class DictionaryExtensions
{
    public static T GetAs<T>(this Dictionary<IndirectReference, DirectObject> dict, IndirectReference key) where T : DirectObject
    {
        if (!dict.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"Key '{key}' was not found in the dictionary.");

        if (value is T result)
            return result;

        throw new InvalidCastException($"The value for key '{key}' is not of type {typeof(T).Name}.");
    }
}