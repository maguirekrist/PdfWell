using System.Diagnostics;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Utils;

public static class PngFilterDecompressor
{


    public static Span<byte> Decompress(Span<byte> buffer, DecoderParams args)
    {
        return args.Predictor switch
        {
            Predictor.PNGUp => ApplyPngFilter(buffer, args.Columns),
            _ => throw new UnreachableException()
        };
    }

    private static Span<byte> ApplyPngFilter(Span<byte> buffer, int columns)
    {
        var bytesPerRow = columns + 1;
        var numberOfRows = buffer.Length / bytesPerRow;
        var upRowData = new byte[bytesPerRow];
        for (var i = 0; i < buffer.Length; i += bytesPerRow)
        {
            var rowData = buffer[i..(bytesPerRow + i)];
            var predictionByte = rowData[0];

            switch (predictionByte)
            {
                case 2:
                    for (var j = 1; j < bytesPerRow; j++)
                    {
                        var upSample = upRowData[j];
                        rowData[j] = (byte)((rowData[j] + upSample) % 256);
                    }
                    break;
                case 0:
                    break;
                default:
                    throw new NotSupportedException($"Unsupported PNG filter type: {predictionByte}");
            }
            
            rowData.CopyTo(upRowData);
        }

        return buffer;
    }
    
}