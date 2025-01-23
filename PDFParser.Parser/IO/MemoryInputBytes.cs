using PDFParser.Parser.Utils;

namespace PDFParser.Parser.IO;

public class MemoryInputBytes
{
    private readonly int _upperbound;
    private readonly ReadOnlyMemory<byte> _memory;

    private int _currentOffset;
    
    public MemoryInputBytes(ReadOnlyMemory<byte> memory)
    {
        _memory = memory;

        _upperbound = _memory.Length - 1;
        _currentOffset = 0;
    }

    public long CurrentOffset => _currentOffset;

    public byte CurrentByte => _memory.Span[_currentOffset];

    public char CurrentChar => (char)CurrentByte;

    public long Length => _memory.Span.Length;
    
    public bool MoveNext()
    {
        if (IsAtEnd())
        {
            return false;
        }

        _currentOffset++;
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
        return _currentOffset >= _upperbound;
    }

    public void Seek(long position)
    {
        _currentOffset = (int)position;
    }

    public int Read(Span<byte> buffer)
    {
        if (buffer.IsEmpty)
        {
            return 0;
        }

        var viableLength = (_memory.Length - _currentOffset - 1);
        var readLength = viableLength < buffer.Length ? viableLength : buffer.Length;
        var startFrom = _currentOffset;
        
        _memory.Span.Slice(startFrom, readLength).CopyTo(buffer);
        
        if (readLength > 0)
        {
            _currentOffset += readLength;
        }

        return readLength;
    }

    public long FindFirstPatternOffset(ReadOnlySpan<byte> matchBytes)
    {
        while (!IsAtEnd())
        {
            if (!CurrentByte.Equals(matchBytes[0]))
            {
                MoveNext();
                continue;
            };

            var offset = CurrentOffset;
            if (Match(matchBytes))
            {
                return offset;
            }
        }

        throw new Exception($"Unable to find byte sequence (ASCII) {matchBytes.ToAscii()}");
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

    public ReadOnlySpan<byte> ReadLine()
    {
        //Assume we are reading at a beginning of a new line
        var begin = CurrentOffset;
        
        var startEol = FindFirstPatternOffset("\n"u8);       

        return _memory.Span.Slice((int)begin, (int)(startEol - begin));
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
        var startEol = FindFirstPatternOffset(option);
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
            if (CurrentByte.IsNumeric() || CurrentByte.IsAlpha() || CurrentByte == '.' || CurrentByte == '#' || CurrentByte == ',' || CurrentByte == '-' || CurrentByte == '&')
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