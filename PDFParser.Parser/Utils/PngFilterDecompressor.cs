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
        var bytesPerRow = columns + 1; // 1 byte filter type + data
        var numberOfRows = buffer.Length / bytesPerRow;
        var output = new byte[columns * numberOfRows];
        var upRow = new byte[columns];

        for (int row = 0; row < numberOfRows; row++)
        {
            int inputOffset = row * bytesPerRow;
            byte filterType = buffer[inputOffset];
            var rowOutputOffset = row * columns;

            switch (filterType)
            {
                case 0: // None
                    buffer.Slice(inputOffset + 1, columns).CopyTo(output.AsSpan(rowOutputOffset));
                    break;

                case 2: // Up
                    for (int col = 0; col < columns; col++)
                    {
                        byte raw = buffer[inputOffset + 1 + col];
                        byte up = upRow[col];
                        output[rowOutputOffset + col] = (byte)((raw + up) & 0xFF);
                    }
                    break;

                default:
                    throw new NotSupportedException($"Unsupported PNG filter type: {filterType}");
            }

            // Save current output row for next row's Up filter
            output.AsSpan(rowOutputOffset, columns).CopyTo(upRow);
        }

        return output;
    }
    
}