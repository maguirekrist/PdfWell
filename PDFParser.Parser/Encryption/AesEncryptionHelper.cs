using System.Security.Cryptography;
using PDFParser.Parser.Crypt;
using PDFParser.Parser.Objects;

namespace PDFParser.Parser.Encryption;

public class AesEncryptionHelper
{
    public static byte[] Decrypt(byte[] buffer, byte[] finalKey)
    {
        var iv = buffer[..16]; // first 16 bytes = Initialization Vector (AES)
        var cipherText = buffer[16..]; // rest = actual encrypted data.
        
        using var aes = Aes.Create();
        aes.Key = finalKey;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        
        return decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
    }
}