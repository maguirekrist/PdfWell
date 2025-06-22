using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Crypt;

public class AESV2DecryptStrategy : IDecryptStrategy
{
    private byte[] _globalKey;   
    public AESV2DecryptStrategy(byte[] globalKey)
    {
        _globalKey = globalKey;
    }
    
    public byte[] DecryptBuffer(byte[] buffer, IndirectReference? reference = null)
    {
        throw new NotImplementedException();
    }
}