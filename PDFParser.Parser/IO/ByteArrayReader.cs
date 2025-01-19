namespace PDFParser.Parser;

public struct ByteArrayReader
{
    private readonly byte[] _data;
    private int _position;

    public ByteArrayReader(byte[] data)
    {
        _data = data;
        _position = 0;
    }

    public bool IsEnd => _position >= _data.Length;

    public byte Current => _data[_position];

    /// <summary>
    /// Advances the internal position by a specified number of bytes.
    /// </summary>
    public void Advance(int count)
    {
        if (_position + count > _data.Length)
            throw new ArgumentOutOfRangeException(nameof(count), "Cannot advance beyond the end of the array.");

        _position += count;
    }

    /// <summary>
    /// Reads a span of bytes up to the next occurrence of a delimiter (e.g., '\n').
    /// </summary>
    public ReadOnlySpan<byte> ReadUntil(byte delimiter)
    {
        int start = _position;

        while (_position < _data.Length && _data[_position] != delimiter)
        {
            _position++;
        }

        if (_position < _data.Length && _data[_position] == delimiter)
        {
            // Include delimiter in span and advance past it
            _position++;
        }

        return _data[start.._position];
    }

    /// <summary>
    /// Reads a span of bytes of a specified length.
    /// </summary>
    public ReadOnlySpan<byte> ReadBytes(int length)
    {
        if (_position + length > _data.Length)
            throw new ArgumentOutOfRangeException(nameof(length), "Cannot read beyond the end of the array.");

        var data = new ReadOnlySpan<byte>(_data);
        var result = data.Slice(_position, length);
        _position += length;
        return result;
    }

    /// <summary>
    /// Skips over any occurrences of specified bytes (e.g., '\r', '\n').
    /// </summary>
    public void Skip(params byte[] bytesToSkip)
    {
        while (_position < _data.Length && Array.IndexOf(bytesToSkip, _data[_position]) != -1)
        {
            _position++;
        }
    }
}