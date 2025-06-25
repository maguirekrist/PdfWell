using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Encryption;

public interface IDecryptStrategy
{
    public byte[] DecryptBuffer(byte[] buffer, IndirectReference? reference = null);
}