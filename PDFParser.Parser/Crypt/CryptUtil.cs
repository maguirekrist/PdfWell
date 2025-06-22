using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Crypt;

public static class CryptUtil
{
    public static byte[] RC4Encrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data)
    {
        var rc4 = new RC4Engine();
        rc4.Init(true, new KeyParameter(key));

        var output = new byte[data.Length];
        rc4.ProcessBytes(data.ToArray(), 0, data.Length, output, 0);
        return output;
    }

    public static byte[] AesV2Decrypt(byte[] encryptedStream, byte[] globalKey, IndirectReference objectLine)
    {
        
        var iv = encryptedStream[..16];
        var ciphertext = encryptedStream[16..];
        var cipher = CipherUtilities.GetCipher("AES/CBC/PKCS7Padding");
        var keyParam = new KeyParameter(globalKey);
        var parameters = new ParametersWithIV(keyParam, iv);

        cipher.Init(false, parameters); // false = decrypt
        return cipher.DoFinal(ciphertext);
    }
}