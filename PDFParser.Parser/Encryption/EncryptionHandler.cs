using System.Security.Cryptography;
using PDFParser.Parser.ConceptObjects;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.Encryption;

public class EncryptionHandler
{
    private static readonly byte[] PaddingBytes =
    [
        0x28, 0xBF, 0x4E, 0x5E,
        0x4E, 0x75, 0x8A, 0x41,
        0x64, 0x00, 0x4E, 0x56,
        0xFF, 0xFA, 0x01, 0x08,
        0x2E, 0x2E, 0x00, 0xB6,
        0xD0, 0x68, 0x3E, 0x80,
        0x2F, 0x0C, 0xA9, 0xFE,
        0x64, 0x53, 0x69, 0x7A
    ];

    private readonly EncryptionDictionary _encryptionDictionary;
    private readonly byte[] _fileID;
    private readonly byte[] _encryptionKey;
    public EncryptionHandler(EncryptionDictionary encryptionDictionary, ArrayObject<DirectObject> fileIdArray)
    {
        _fileID = fileIdArray.GetAs<StringObject>(0).Value;
        _encryptionDictionary = encryptionDictionary;
        _encryptionKey = GetGlobalEncryptionKey([]);
    }
    
    // public bool CanDecrypt(string password = "")
    // {
    //     //TODO: Handle password
    //     //TODO: Password needs to be PDFDocEncoding... we should have that as public static function.
    //     var encryptionKey = DeriveKeyFromPassword(password);
    //     var trialU = CryptUtil.RC4Encrypt(encryptionKey, PaddingBytes);
    //     
    //     if (trialU.SequenceEqual(UserKey.Value))
    //     {
    //         throw new Exception("YESS!!");
    //         return true;
    //     }
    //
    //     if (trialU.SequenceEqual(OwnerKey.Value))
    //     {
    //         throw new Exception("USER!");
    //     }
    //
    //
    //     return false;
    // }
    
    private byte[] GetPaddedPassword(byte[] password)
    {
        if (password.Length == 0)
        {
            return PaddingBytes;
        }
        
        var first32 = password.AsSpan(0, Math.Min(32, password.Length));
        var padding = PaddingBytes[..(32-first32.Length)];
        return BinaryHelper.Combine(first32.ToArray(), padding.ToArray());
    }

    private static bool IsOwnerPassword()
    {
        //TODO:
        return false;
    }

    private static bool IsUserPassword()
    {
        //TODO:
        return false;
    }

    private byte[] GetObjectKey(IndirectReference reference)
    {
        var bytesObjNumber = BitConverter.GetBytes(reference.ObjectNumber)[..3];
        var bytesGenNumber = BitConverter.GetBytes(reference.Generation)[..2];
        var saltBytes = new byte[] { (byte)'s', (byte)'A', (byte)'l', (byte)'T' };
        //Take the low order bytes of 
        var newKey = new byte[_encryptionKey.Length + 9];
        Buffer.BlockCopy(_encryptionKey, 0, newKey, 0, _encryptionKey.Length);
        Buffer.BlockCopy(bytesObjNumber, 0, newKey, _encryptionKey.Length, 3);
        Buffer.BlockCopy(bytesGenNumber, 0, newKey, _encryptionKey.Length + 3, 2);

        if (true)
        {
            Buffer.BlockCopy(saltBytes, 0, newKey, _encryptionKey.Length + 5, 4);
        }
        
        //Span method... not going to use as MD5 doesn't support spans for some reason.
        // _globalKey.CopyTo(newKey);
        // bytesObjNumber.CopyTo(newKey.Slice(_globalKey.Length, 3));
        // bytesGenNumber.CopyTo(newKey.Slice(_globalKey.Length + 3, 2));

        using var md5 = MD5.Create();
        var length = Math.Min(16, _encryptionKey.Length + 5);
        var hash = md5.ComputeHash(newKey)[..length];
        return hash;
    }
    
    public byte[] Decrypt(byte[] data, IndirectReference reference)
    {
        var finalKey = GetObjectKey(reference);

        return AesEncryptionHelper.Decrypt(data, finalKey);
    }
    
    public byte[] GetGlobalEncryptionKey(byte[] password)
    {
        var length = 16;
        
        var passwordFull = GetPaddedPassword(password);
        
        var ownerEntry = _encryptionDictionary.OwnerKey.Value;

        using (var md5 = MD5.Create())
        {
            UpdateMd5(md5, passwordFull);
            
            UpdateMd5(md5, _encryptionDictionary.OwnerKey.Value);
            
            UpdateMd5(md5, BitConverter.GetBytes((int)_encryptionDictionary.PermissionFlags.Value));

            UpdateMd5(md5, _fileID);

            if (_encryptionDictionary.Revision >= 4 && !_encryptionDictionary.EncryptMetadata!.Value)
            {
                UpdateMd5(md5, [0xFF, 0xFF, 0xFF, 0xFF]);
            }

            if (_encryptionDictionary.Revision is 3 or 4)
            {
                var n = length;
                
                md5.TransformFinalBlock([], 0, 0);

                var input = md5.Hash;
                using (var newMd5 = MD5.Create())
                {
                    for (var i = 0; i < 50; i++)
                    {
                        input = newMd5.ComputeHash(input.AsSpan(0, n).ToArray());
                    }
                }

                var result = new byte[length];
                
                Array.Copy(input, result, 16);

                return result;
            }
            else
            {
                md5.TransformFinalBlock([], 0, 0);

                var result = new byte[length];

                Array.Copy(md5.Hash, result, length);

                return result;
            }
        }
        var permissionBytes = BitConverter.GetBytes((int)_encryptionDictionary.PermissionFlags.Value);

        var metadataFlag = BitConverter.GetBytes(0xFFFFFFFF);

        var binaryBuilder = new BinaryBuilder();

        binaryBuilder.Add(passwordFull);
        binaryBuilder.Add(ownerEntry);
        binaryBuilder.Add(permissionBytes);
        binaryBuilder.Add(_fileID);

        if (_encryptionDictionary.EncryptMetadata is { Value: false })
        {
            binaryBuilder.Add(metadataFlag);
        }

        var keyData = binaryBuilder.Build();

        //using var md5 = MD5.Create();
        //var hash = md5.ComputeHash(keyData);
        //This varies depending on the dictionary settings.
        
        //var keyLength = 16;
        // if (_encryptionDictionary.Revision is 3 or 4)
        // {
        //     using (var newMd5 = MD5.Create())
        //     {
        //         for (var i = 0; i < 50; i++)
        //         {
        //             hash = newMd5.ComputeHash(hash.AsSpan(0, keyLength).ToArray());
        //         }
        //     }
        // }
        //
        // var encryptionKey = hash[..keyLength];
        // return encryptionKey;
    }

    private static void UpdateMd5(MD5 md5, byte[] data)
    {
        md5.TransformBlock(data, 0, data.Length, null, 0);
    }
}