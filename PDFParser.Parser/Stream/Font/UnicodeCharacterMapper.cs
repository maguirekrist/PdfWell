using System.Text;
using PDFParser.Parser.IO;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Stream.Font;

public class UnicodeCharacterMapper : IEncoding
{
    private Dictionary<ushort, char> _lookupTable = new();
    private SortedList<ushort, (ushort End, ushort UnicodeStart)> _ranges = new();
    
    public UnicodeCharacterMapper(MemoryInputBytes stream)
    {
        Init(stream);
    }

    public string GetString(ReadOnlySpan<byte> bytes)
    {
        var builder = new StringBuilder(bytes.Length / 2);
        for (var i = 0; i < bytes.Length; i += 2)
        {
            ushort pack = (ushort)((bytes[i] << 8) | bytes[i + 1]);
            builder.Append(Lookup(pack));
        }

        return builder.ToString();
    }

    private void Init(MemoryInputBytes cmapStream)
    {
        var parser = new PdfParser(cmapStream);
        //1. get the codespace range
        cmapStream.ReadUntil("begincodespacerange"u8);
        cmapStream.SkipWhitespace();
        var begin = parser.ParseStringObject(cmapStream);
        cmapStream.SkipWhitespace();
        var end = parser.ParseStringObject(cmapStream);
        var codePointSize = begin.Value.Length;

        cmapStream.ReadUntil("beginbfchar"u8);
        while (!cmapStream.IsAtEnd() && cmapStream.CurrentChar != 'e')
        {
            cmapStream.SkipWhitespace();
            var from = parser.ParseStringObject(cmapStream);
            cmapStream.SkipWhitespace();
            var to = parser.ParseStringObject(cmapStream);
            cmapStream.SkipWhitespace();
            AddCharacterCode(from, to);
        }

        cmapStream.ReadUntil("beginbfrange"u8);
        while (!cmapStream.IsAtEnd() && cmapStream.CurrentChar != 'e')
        {
            cmapStream.SkipWhitespace();
            var from = parser.ParseStringObject(cmapStream);
            cmapStream.SkipWhitespace();
            var to = parser.ParseStringObject(cmapStream);
            cmapStream.SkipWhitespace();
            var unicodeBegin = parser.ParseStringObject(cmapStream);
            cmapStream.SkipWhitespace();
            AddCharacterRange(from, to, unicodeBegin);
        }
    }

    private void AddCharacterCode(StringObject key, StringObject value)
    {
        if (key.Value.Length != 2 || value.Value.Length != 2)
        {
            throw new ArgumentException();
        }
        
        var pack = (ushort)((key.Value[0] << 8) | key.Value[1]);
        var valPack = (char)((value.Value[0] << 8) | value.Value[1]);
        _lookupTable.Add(pack, valPack);
    }

    private void AddCharacterRange(StringObject from, StringObject to, StringObject begin)
    {
        if (from.Value.Length != 2 || to.Value.Length != 2 || begin.Value.Length != 2)
        {
            throw new ArgumentException();
        }
        
        var fromVal = (ushort)((from.Value[0] << 8) | from.Value[1]);
        var toVal = (ushort)((to.Value[0] << 8) | to.Value[1]);
        var beginVal = (ushort)((begin.Value[0] << 8) | begin.Value[1]);

        _ranges.Add(fromVal, (toVal, beginVal));
    }

    private char Lookup(ushort characterCode)
    {
        if (_lookupTable.TryGetValue(characterCode, out var result)) return result;
        
        var keys = _ranges.Keys;
        var low = 0;
        var high = keys.Count - 1;

        while (low <= high)
        {
            int mid = (low + high) / 2;

            if (keys[mid] <= characterCode)
            {
                if (_ranges[keys[mid]].End >= characterCode)
                {
                    //Found
                    return (char)(_ranges[keys[mid]].UnicodeStart + (characterCode - keys[mid]));
                }

                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        throw new KeyNotFoundException();

    }
}