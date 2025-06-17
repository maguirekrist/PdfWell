using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using Dynamitey.DynamicObjects;
using PDFParser.Parser.Document;
using PDFParser.Parser.IO;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Objects;

public enum StreamFilter
{
    None,
    Flate
}

public enum Predictor
{
    None = 1,
    TIFF = 2,
    PNGNone = 10,
    PNGSub = 11,
    PNGUp = 12,
    PNGAverage = 13,
    PNGPaeth = 14,
    PNGOptimum = 15
}

public struct DecoderParams
{
    public Predictor Predictor;
    public int Columns;
}

public class StreamObject : DictionaryObject
{
    //Note -- Content Streams
    //PDF Content Streams apart of the PDF syntax but have a unique syntax introduction called operator
    //Streams use Reverse Polish notation, so you see operands followed by operators
    //Operands are expressed as standard PDF objects -- i.e. Array Objects, Named Objects, Numeric Objects, String Objects
    //Operators are special codes that do specific operations, i.e. sets the type face, prints text, sets color, etc.
    

    //This class is simply a wrapper for the stream data... Data represents the contents between stream and endstream
    //The class user is responsible for making sense of the stream.

    private ReadOnlyMemory<byte> _data; //This is Raw Data.

    private readonly System.Lazy<ReadOnlyMemory<byte>> _decodedStream;
    public ReadOnlyMemory<byte> DecodedStream => _decodedStream.Value;

    public MemoryInputBytes Reader => new(_decodedStream.Value);

    public StreamFilter Filter => GetFilter();

    public DecoderParams? DecoderParams => GetDecoderParams();
    
    public StreamObject(ReadOnlyMemory<byte> buffer, DictionaryObject streamDictionary) : base(streamDictionary)
    {
        _data = buffer;
        _decodedStream = new System.Lazy<ReadOnlyMemory<byte>>(GetDecodedStream);
    }

    public StreamObject(StreamObject obj) : this(obj._data, obj)
    {
    }

    private DecoderParams? GetDecoderParams()
    {
        if (Filter != StreamFilter.Flate) return null;

        var paramDictionary = TryGetAs<DictionaryObject>("DecodeParms");
        if (paramDictionary == null) return null;

        var columnObj = paramDictionary.TryGetAs<NumericObject>("Columns");
        var predictorObj = paramDictionary.TryGetAs<NumericObject>("Predictor");

        if (columnObj != null && predictorObj != null)
        {
            return new DecoderParams { Columns = (int)columnObj.Value, Predictor = (Predictor)((int)predictorObj.Value) };
        }

        throw new UnreachableException();
    }
    
    private StreamFilter GetFilter()
    {
        var filterObject = TryGetAs<NameObject>("Filter");

        return filterObject?.Name switch
        {
            Filters.FlateEncoding => StreamFilter.Flate,
            _ => StreamFilter.None
        };
    }

    public ReadOnlyMemory<byte> GetDecodedStream()
    {
        switch (Filter)
        {
            case StreamFilter.None:
                return _data;
            case StreamFilter.Flate:
            {
                //var rawBytes = Data.ToArray();
                // if (Data[..2].Span.SequenceEqual(""))
                var rawBytes = _data.ToArray()[2..];
                using var memoryStream = new MemoryStream(rawBytes);
                using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
                using var output = new MemoryStream();
                deflateStream.CopyTo(output);

                var decodedBytes = output.ToArray().AsSpan();
                if (DecoderParams.HasValue)
                {
                    decodedBytes = PngFilterDecompressor.Decompress(decodedBytes, DecoderParams.Value);
                }
                return decodedBytes.ToArray();
            }
            default:
                throw new UnreachableException();
        }
    }
    
    
}