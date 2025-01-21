using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;
using System.Diagnostics;

namespace PDFParser.Parser.Stream.Text;

internal class SetTextMatrix : ITextCommand
{

    private readonly int[] _matrix;
    
    internal SetTextMatrix(IReadOnlyList<DirectObject> objects)
    {
        if (objects.Count != 6)
        {
            throw new ArgumentException($"Object list length expected to be 6. Received objects with length: {objects.Count}");
        }

        _matrix = objects.OfType<NumericObject>().Select(x => (int)x.Value).ToArray();
    }

    public void Execute(TextState textState)
    {
        textState.Matrix = _matrix;
    }
}
