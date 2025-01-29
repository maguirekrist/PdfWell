using System.Text;

namespace PDFParser.Parser.IO;

public class BoyerMooreMatcher : IMatcher
{
    public long? FindFirstOffset(ReadOnlyMemory<byte> stream, ReadOnlySpan<byte> pattern)
    {
        if (pattern.Length == 0) return null;
        if (stream.Length < pattern.Length) return null;
        
        var shiftTable = new int[256];
        var m = pattern.Length;
        var n = stream.Length;
        // Build Shift Table
        for (var i = 0; i < 256; i++)
            shiftTable[i] = m + 1;
        for (var i = 0; i < m; i++)
            shiftTable[pattern[i]] = m - i;

        var s = 0;
        while (s <= n - m)
        {
            var j = 0;
            while (j < m && stream.Span[s + j] == pattern[j])
                j++;

            if (j == m)
                return s; // Match found

            if (s + m < n)
                s += shiftTable[stream.Span[s + m]];
            else
                break;
        }
        return null; // Not found

        //throw new Exception($"Pattern: {Encoding.Default.GetString(pattern)} not found in byte stream.");
    }
}