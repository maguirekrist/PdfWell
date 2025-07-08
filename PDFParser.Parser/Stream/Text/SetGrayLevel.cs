using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Stream.Text;

internal class SetGrayLevel : ITextCommand
{
    public double Value { get; }
    
    public bool IsStroking { get; }

    public SetGrayLevel(NumericObject value, bool isStroking = false)
    {
        Value = value.Value;
        IsStroking = isStroking;
    }
    
    public void Execute(TextState textState)
    {
        //TODO: Figure out if this should execute anything... probably not needed for a parser, we don't display anything.
        //no-op for now....
    }
}