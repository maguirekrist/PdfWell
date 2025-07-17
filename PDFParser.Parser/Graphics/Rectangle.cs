using Org.BouncyCastle.Security;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Graphics;

public class Rectangle
{
    public float Height { get; }
    public float Width { get; }
    public float X { get; }
    public float Y { get; }

    public Rectangle(float height, float width, float x, float y)
    {
        Height = height;
        Width = width;
        X = x;
        Y = y;
    }

    public static Rectangle FromPdfArray(ArrayObject<DirectObject> arrayObject)
    {
        if (arrayObject.Count > 4) throw new InvalidParameterException("Invalid arrayObject, must be a valid PDF array with a length of 4.");
        
        var llx = (float) arrayObject.GetAs<NumericObject>(0).Value;
        var lly = (float) arrayObject.GetAs<NumericObject>(1).Value;
        var llx2 = (float) arrayObject.GetAs<NumericObject>(2).Value;
        var lly2 = (float) arrayObject.GetAs<NumericObject>(3).Value;

        return new Rectangle(lly2 - lly, llx2 - llx, llx, lly);
    }
}