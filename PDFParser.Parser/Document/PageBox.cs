namespace PDFParser.Parser.Document;

public readonly record struct PageBox(int Left, int Bottom, int Width, int Height)
{
    public PageBox(int[] options) : this(options[0], options[1], options[2], options[3])
    {
    }
}