using System.Text;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.IO;

public class MemoryInputBytes
{
    private readonly int _upperbound;
    private readonly ReadOnlyMemory<byte> _memory;

    private IMatcher? _matchStrategy;
    private int _currentOffset;
    
    public MemoryInputBytes(ReadOnlyMemory<byte> memory, IMatcher? matchStrategy = null)
    {
        _memory = memory;
        _upperbound = _memory.Length - 1;
        _currentOffset = 0;
        _matchStrategy = matchStrategy;
    }

    public long CurrentOffset => _currentOffset;

    public byte CurrentByte => _currentOffset > _upperbound ? _memory.Span[_upperbound] : _memory.Span[_currentOffset];

    public char CurrentChar => (char)CurrentByte;

    public long Length => _memory.Span.Length;

    public byte[] Data => _memory.ToArray();
    
    public bool MoveNext()
    {
        if (IsAtEnd())
        {
            return false;
        }

        _currentOffset++;
        return true;
    }

    public bool Move(int amount)
    {
        if (IsAtEnd() || _currentOffset + amount >= _upperbound)
        {
            return false;
        }

        _currentOffset += amount;
        return true;
    }

    public bool StepBack()
    {
        if (_currentOffset == 0)
        {
            return false;
        }

        _currentOffset--;
        return true;
    }

    public byte? Peek()
    {
        if (_currentOffset == _upperbound)
        {
            return null;
        }

        return _memory.Span[_currentOffset + 1];
    }

    public byte? LookBehind(int numberOfBytes)
    {
        if (_currentOffset - numberOfBytes < 0)
        {
            return null;
        }

        return _memory.Span[_currentOffset - numberOfBytes];
    }

    public byte? LookAhead(int numberOfBytes)
    {
        if (_currentOffset + numberOfBytes >= _upperbound)
        {
            return null;
        }

        return _memory.Span[_currentOffset + numberOfBytes];
    }

    public bool IsAtEnd()
    {
        return _currentOffset > _upperbound;
    }

    public bool IsAtNewLine()
    {
        List<byte[]> newLines = [Encoding.ASCII.GetBytes("\n"), Encoding.ASCII.GetBytes("\r"), Encoding.ASCII.GetBytes("\r\n")];
            
        foreach (var delimiter in newLines)
        {
            if (_memory.Slice(_currentOffset, delimiter.Length).Span.SequenceEqual(delimiter))
            {
                return true;
            }
        }

        return false;
    }

    public void Seek(long position)
    {
        _currentOffset = (int)position;
    }

    private int Read(Span<byte> buffer)
    {
        if (buffer.IsEmpty)
        {
            return 0;
        }

        var viableLength = (_memory.Length - _currentOffset);
        var readLength = viableLength < buffer.Length ? viableLength : buffer.Length;
        var startFrom = _currentOffset;
        
        _memory.Span.Slice(startFrom, readLength).CopyTo(buffer);
        
        // if (readLength > 0)
        // {
        //     _currentOffset += readLength;
        // }

        return readLength;
    }

    public long? FindFirstPatternOffset(ReadOnlySpan<byte> matchBytes, int? maxBytesToRead = null)
    {
        if (_matchStrategy is not null)
        {
            var begin = _currentOffset;
            var offset= _matchStrategy.FindFirstOffset(_memory.Slice(_currentOffset), matchBytes);
            if (offset is not null)
            {
                _currentOffset = (int)offset + begin + matchBytes.Length;
                return offset + begin;
            }
            return null;
        }
        
        var bytesRead = 0;
        while (!IsAtEnd())
        {
            if (maxBytesToRead != null && bytesRead > maxBytesToRead)
            {
                return null;
            }
            
            if (!CurrentByte.Equals(matchBytes[0]))
            {
                MoveNext();
                bytesRead++;
                continue;
            }

            var offset = CurrentOffset;
            if (Match(matchBytes))
            {
                _currentOffset += matchBytes.Length;
                return offset;
            }
            else
            {
                bytesRead++;
                MoveNext();
            }
        }

        return null;
    }

    public long? FindFirstPatternOffset(List<byte[]> matchBytes, int? maxBytesToRead = null)
    {
        var indx = IndexOfAny(matchBytes, maxBytesToRead, out var matchLength);
        if (indx == -1)
        {
            return null;
        }

        _currentOffset = indx + matchLength;
        return indx;
    }

    private int IndexOfAny(List<byte[]> delimiters, int? maxBytesToRead, out int matchLength)
    {
        for (int i = _currentOffset; i < Length; i++)
        {
            if (maxBytesToRead != null && i > maxBytesToRead)
            {
                break;
            }
            
            foreach (var delimiter in delimiters)
            {
                if (delimiter.Length == 0 || i + delimiter.Length > Length)
                    continue;

                if (_memory.Slice(i, delimiter.Length).Span.SequenceEqual(delimiter))
                {
                    matchLength = delimiter.Length;
                    return i;
                }
            }
        }

        matchLength = 0;
        return -1;
    }

    public long? RewindUntilFirstFound(List<byte[]> delimiters, int? maxBytesToRead, out int matchLength)
    {
        var readBytes = 0;
        for (var i = _currentOffset; i > 0; i--)
        {
            if (maxBytesToRead != null && readBytes > maxBytesToRead)
            {
                break;
            }
            
            foreach (var delimiter in delimiters)
            {
                if (delimiter.Length == 0 || i + delimiter.Length > Length)
                {
                    continue;
                }

                if (_memory.Slice(i, delimiter.Length).Span.SequenceEqual(delimiter))
                {
                    matchLength = delimiter.Length;
                    return i;
                }
            }

            readBytes++;
        }

        matchLength = 0;
        return null;
    }
    
    public long RewindUntil(ReadOnlySpan<byte> matchBytes)
    {
        while (_currentOffset != 0)
        {
            if (!CurrentByte.Equals(matchBytes[^1]))
            {
                StepBack();
                continue;
            }
            
            _currentOffset -= matchBytes.Length - 1;
            var offset = _currentOffset;
            if (Match(matchBytes))
            {
                Seek(offset + 1);
                return offset + 1;
            }
        }
        
        throw new Exception($"Unable to find byte sequence (ASCII) {matchBytes.ToAscii()}");
    }

    public bool Match(ReadOnlySpan<byte> matchBytes)
    {
        if (matchBytes.IsEmpty || (_memory.Length - _currentOffset) < matchBytes.Length || IsAtEnd())
        {
            return false;
        }

        Span<byte> tempBuffer = stackalloc byte[matchBytes.Length];
        
        if (Read(tempBuffer) != matchBytes.Length)
        {
            return false;
        }
        
        
        return tempBuffer.SequenceEqual(matchBytes);
    }

    public void GotoBeginLine()
    {
        while (IsAtNewLine())
        {
            _currentOffset -= 1;
        }
        
        var begin = RewindUntilFirstFound(
            [Encoding.ASCII.GetBytes("\n"), Encoding.ASCII.GetBytes("\r"), Encoding.ASCII.GetBytes("\r\n")], null,
            out var matchLength);
        if (begin == null)
        {
            throw new Exception("WTF!");
        }

        _currentOffset = (int)begin + matchLength;
    }

    //Gets the current line the current offset is on... unlike readLine, which reads to the end of line.
    public ReadOnlySpan<byte> GetLine()
    {
        while (IsAtNewLine())
        {
            _currentOffset -= 1;
        }
        
        var begin = RewindUntilFirstFound(
            [Encoding.ASCII.GetBytes("\n"), Encoding.ASCII.GetBytes("\r"), Encoding.ASCII.GetBytes("\r\n")], null,
            out var matchLength);
        if (begin == null)
        {
            throw new Exception("WTF!");
        }

        begin += matchLength;
            
        var startEol = FindFirstPatternOffset([
            Encoding.ASCII.GetBytes("\n"), Encoding.ASCII.GetBytes("\r"), Encoding.ASCII.GetBytes("\r\n")
        ]) ?? _currentOffset;      
        
        return _memory.Span.Slice((int)begin, (int)(startEol - begin));
    }

    public ReadOnlySpan<byte> ReadLine()
    {
        //Assume we are reading at a beginning of a new line
        var begin = _currentOffset;
        
        var startEol = FindFirstPatternOffset([
            Encoding.ASCII.GetBytes("\n"), Encoding.ASCII.GetBytes("\r"), Encoding.ASCII.GetBytes("\r\n")
        ]) ?? _currentOffset;      
        
        return _memory.Span.Slice((int)begin, (int)(startEol - begin));
    }
    
    public void NextLine()
    {
        var startEol = FindFirstPatternOffset([
            Encoding.ASCII.GetBytes("\n"), Encoding.ASCII.GetBytes("\r"), Encoding.ASCII.GetBytes("\r\n")
        ]) ?? _currentOffset;

        if (IsAtNewLine())
        {
            MoveNext();
            //Handles \r\n format
            if (CurrentChar == '\n')
            {
                MoveNext();
            }
        }
    }

    public void SkipWhitespace()
    {
        //Skips all "whitespace" ASCII codes
        Skip(0, 9, 10, 11, 12, 13, 32);
    }

    public void Skip(params byte[] bytesToSkip)
    {
        while (!IsAtEnd() && Array.IndexOf(bytesToSkip, CurrentByte) != -1)
        {
            MoveNext();
        }
    }

    public ReadOnlySpan<byte> ReadUntil(ReadOnlySpan<byte> option)
    {
        var begin = CurrentOffset;
        var startEol = FindFirstPatternOffset(option) ?? throw new Exception($"End of File reached while attempting find sequence: {Encoding.ASCII.GetString(option)}");
        
        return _memory.Span.Slice((int)begin, (int)(startEol - begin));
    }

    public ReadOnlySpan<byte> ReadAlpha()
    {
        var begin = CurrentOffset;
        while (!IsAtEnd())
        {
            if (!CurrentByte.IsAlpha())
            {
                return _memory.Span.Slice((int) begin, (int)(CurrentOffset - begin));
            };

            MoveNext();
        }

        throw new Exception($"Alpha run continued to the end of buffer.");
    }

    public ReadOnlySpan<byte> ReadNumeric()
    {
        var begin = CurrentOffset;
        while (!IsAtEnd())
        {
            if (!CurrentByte.IsNumeric())
            {
                return _memory.Span.Slice((int) begin, (int)(CurrentOffset - begin));
            }

            MoveNext();
        }

        throw new Exception("Numeric run continued to the end of buffer.");
    }

    //Special Function for PDF NameObject Naming Rules
    public ReadOnlySpan<byte> ReadName()
    {
        var begin = CurrentOffset;
        while (!IsAtEnd())
        {
            if (CurrentByte.IsNumeric() || CurrentByte.IsAlpha() || CurrentByte == ':' || CurrentByte == '_' || CurrentByte == '.' || CurrentByte == '#' || CurrentByte == ',' || CurrentByte == '-' || CurrentByte == '&')
            {
                MoveNext();
            }
            else
            {
                return _memory.Span.Slice((int) begin, (int)(CurrentOffset - begin));
            }
        }

        throw new Exception("Numeric run continued to the end of buffer.");
    }

    public ReadOnlyMemory<byte> Slice(int offset, int length)
    {
        return _memory.Slice(offset, length);
    }
}