using System.Text;
using PDFParser.Parser.IO;
using PDFParser.Parser.Objects;
using PDFParser.Parser.String;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Stream.Font;

public class UnicodeCharacterMapper : IEncoding
{
    private Dictionary<uint, UnicodeChar> _lookupTable = new();
    private SortedList<uint, (uint End, uint UnicodeStart)> _ranges = new();
    
    public UnicodeCharacterMapper(MemoryInputBytes stream)
    {
        Init(stream);
    }
    
    
    public string GetString(ReadOnlySpan<byte> bytes)
    {
        var builder = new StringBuilder();
        var cursor = 0;
        while (cursor < bytes.Length)
        {
            for (var len = 1; len <= 4 && cursor + len <= bytes.Length; len++)
            {
                var pack = BinaryHelper.PackBytesBigEndian(bytes.Slice(cursor, len));
                var found = Lookup(pack);

                if (found.HasValue)
                {
                    builder.Append(found.Value.Value);
                    break;
                }
                else if (len == 4)
                {
                    builder.Append((char)bytes[cursor]);
                    break;
                }
            }

            cursor++;
        }

        return builder.ToString();
    }

    private void Init(MemoryInputBytes cmapStream)
    {
        //1. get the codespace range
        cmapStream.ReadUntil("begincodespacerange"u8);
        cmapStream.SkipAllWhitespace();
        var begin = PdfParser.ParseStringObject(cmapStream);
        cmapStream.SkipAllWhitespace();
        var end = PdfParser.ParseStringObject(cmapStream);
        var codePointSize = begin.Value.Length;
        
        cmapStream.Seek(0);
        
        cmapStream.ReadUntil("beginbfchar"u8);
        cmapStream.GotoBeginLine();
        var charCounts = int.Parse(Encoding.ASCII.GetString(cmapStream.ReadNumeric()));
        cmapStream.NextLine();
        if (charCounts > 0)
        {
            while (!cmapStream.IsAtEnd() && cmapStream.CurrentChar != 'e')
            {
                cmapStream.SkipAllWhitespace();
                var from = PdfParser.ParseStringObject(cmapStream);
                cmapStream.SkipAllWhitespace();
                var to = PdfParser.ParseStringObject(cmapStream);
                cmapStream.SkipAllWhitespace();
                AddCharacterCode(from, to);
            }   
        }

        cmapStream.Seek(0);

        cmapStream.ReadUntil("beginbfrange"u8);
        cmapStream.GotoBeginLine();
        var rangeCounts = int.Parse(Encoding.ASCII.GetString(cmapStream.ReadNumeric()));
        cmapStream.NextLine();
        if (rangeCounts > 0)
        {
            while (!cmapStream.IsAtEnd() && cmapStream.CurrentChar != 'e')
            {
                cmapStream.SkipAllWhitespace();
                var from = PdfParser.ParseStringObject(cmapStream);
                cmapStream.SkipAllWhitespace();
                var to = PdfParser.ParseStringObject(cmapStream);
                cmapStream.SkipAllWhitespace();
                var unicodeBegin = PdfParser.ParseStringObject(cmapStream);
                cmapStream.SkipAllWhitespace();
                AddCharacterRange(from, to, unicodeBegin);
            }   
        }
    }

    private void AddCharacterCode(StringObject key, StringObject value)
    {
        var pack = BinaryHelper.PackBytesBigEndian(key.Value); //pack big endian
        _lookupTable.Add(pack, new UnicodeChar(value.Value));
    }

    private void AddCharacterRange(StringObject from, StringObject to, StringObject begin)
    {
        var fromValOld = from.Value.Length == 1 ? from.Value[0] : (ushort)((from.Value[0] << 8) | from.Value[1]);
        var toValOld = to.Value.Length == 1 ? to.Value[0] : (ushort)((to.Value[0] << 8) | to.Value[1]);
        var fromVal = BinaryHelper.PackBytesBigEndian(from.Value);
        var toVal = BinaryHelper.PackBytesBigEndian(to.Value);
        var beginVal = BinaryHelper.PackBytesBigEndian(begin.Value);

        _ranges.Add(fromVal, (toVal, beginVal));
    }

    private UnicodeChar? Lookup(uint characterCode)
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
                    return UnicodeChar.FromCodePoint(_ranges[keys[mid]].UnicodeStart + (characterCode - keys[mid]));
                }

                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return null;

    }
}