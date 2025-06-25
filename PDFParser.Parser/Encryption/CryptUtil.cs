using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace PDFParser.Parser.Encryption;

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

    public static byte[] AesV2Decrypt(byte[] key, byte[] iv, byte[] cipherText)
    {
        var cipher = CipherUtilities.GetCipher("AES/CBC/PKCS7Padding");
        var keyParam = new KeyParameter(key);
        var parameters = new ParametersWithIV(keyParam, iv);

        cipher.Init(false, parameters); // false = decrypt
        return cipher.DoFinal(cipherText);
    }
}