namespace PDFParser.Parser.Encryption;


//PDF integer objects can be interpreted as binary values in a signed-twos complement form.
//Since all the reserved high-order flags bits in the permission flags are required to be 1.
//the integer value always appears as a negative number.
//

[Flags]
public enum UserAccessPermissions
{
    PrintDocument = 1 << 3,
    ModifyContents = 1 << 4,
    CopyExtract = 1 << 5,
    AnnotateText = 1 << 6,
    FillForms = 1 << 9,
    AssembleDocument = 1 << 11,
    QualityPrint = 1 << 12
}