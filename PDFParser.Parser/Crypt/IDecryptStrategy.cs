using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Crypt;

public interface IDecryptStrategy
{
    public byte[] DecryptBuffer(byte[] buffer, IndirectReference? reference = null);
}