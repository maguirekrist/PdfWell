using System.Text;

namespace PDFParser.Parser.IO;

public class KMPByteMatcher : IMatcher
{
    public int? FindFirstOffset(ReadOnlyMemory<byte> stream, ReadOnlySpan<byte> pattern)
    {
        if (pattern.Length == 0 || stream.Length < pattern.Length) return null;

        int[] lps = ComputeLPS(pattern);
        int t = 0; //the position of the current character in the stream
        int p = 0; //the position of the current character in the pattern

        while (t < stream.Length)
        {
            if (pattern[p] == stream.Span[t])
            {
                t++;
                p++;

                if (p == pattern.Length)
                {
                    //Occurrence found, if only first occurrence is needed then you could halt here.
                    return t - p;
                }
            }
            else
            {
                if (p != 0)
                {
                    p = lps[p - 1];
                }
                else
                {
                    t++;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Computes the LPS (Longest Prefix Suffix) of the pattern
    /// </summary>
    /// <param name="pattern">byte pattern</param>
    /// <returns>int[] Longest Prefix Suffix array</returns>
    private static int[] ComputeLPS(ReadOnlySpan<byte> pattern)
    {
        int m = pattern.Length;
        int[] lps = new int[m];
        int length = 0;
        int i = 1;

        while (i < m)
        {
            if (pattern[i] == pattern[length])
            {
                length++;
                lps[i] = length;
                i++;
            }
            else
            {
                if (length != 0)
                {
                    length = lps[length - 1];
                }
                else
                {
                    lps[i] = 0;
                    i++;
                }
            }
        }

        return lps;
    }
}