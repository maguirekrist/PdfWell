using System.Text;
using PDFParser.Parser.IO;

namespace PDFParser.Parser.Exceptions;

public class UnexpectedTokenException : Exception
{
    public UnexpectedTokenException(MemoryInputBytes inputBytes)
    : base(BuildTokenDebugMessage(inputBytes))
    {
    }

    public UnexpectedTokenException(MemoryInputBytes inputBytes, Exception ex)
    : base(BuildTokenDebugMessage(inputBytes), ex)
    {
    }

    private static string BuildTokenDebugMessage(MemoryInputBytes inputBytes)
    {
        //Get the whole line....
        var failureOffset = inputBytes.CurrentOffset;
        var failureToken = inputBytes.CurrentChar;
        //Essentially read the whole line...
        var begin = inputBytes.RewindUntil(stackalloc byte[] { 10 });
        var line = inputBytes.ReadUntil(stackalloc byte[] { 10 });

        var builder = new StringBuilder();

        builder.AppendLine($"Unexpected token: {failureToken} while parsing.");
        builder.AppendLine($"Line: {Encoding.ASCII.GetString(line)}");
        builder.AppendLine(Encoding.ASCII.GetString(inputBytes.Slice((int)begin, (int)(failureOffset - begin)).Span));
        builder.AppendLine(
            "^".PadLeft((int)(failureOffset - (begin + 1)))
            );
        return builder.ToString();
    }
}