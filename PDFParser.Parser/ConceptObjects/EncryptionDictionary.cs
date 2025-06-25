using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using PDFParser.Parser.Attributes;
using PDFParser.Parser.Crypt;
using PDFParser.Parser.Encryption;
using PDFParser.Parser.Objects;
using PDFParser.Parser.Utils;

namespace PDFParser.Parser.ConceptObjects;

/// <summary>
/// The method used by the consumer application to decrypt data.
/// </summary>
public enum Method
{
    /// <summary>
    /// The application does not decrypt data but directs the input stream
    /// to the security handler for decryption.
    /// </summary>
    None,
    /// <summary>
    /// The application asks the security handler for the encryption key
    /// and implicitly decrypts data using the RC4 algorithm.
    /// </summary>
    V2,
    /// <summary>
    /// (PDF 1.6) The application asks the security handler for the encryption key and implicitly decrypts data using the AES algorithm in Cipher Block Chaining (CBC) mode
    /// with a 16-byte block size and an initialization vector that is randomly generated and placed as the first 16 bytes in the stream or string. 
    /// </summary>
    AesV2,
    /// <summary>
    /// The application asks the security handler for the encryption key and implicitly decrypts data using the AES-256 algorithm in Cipher Block Chaining (CBC) with padding mode 
    /// with a 16-byte block size and an initialization vector that is randomly generated and placed as the first 16 bytes in the stream or string. 
    /// The key size shall be 256 bits.
    /// </summary>
    AesV3
}

/// <summary>
/// The event to be used to trigger the authorization that is required
/// to access encryption keys used by this filter. 
/// </summary>
public enum TriggerEvent
{
    /// <summary>
    /// Authorization is required when a document is opened.
    /// </summary>
    DocumentOpen,
    /// <summary>
    /// Authorization is required when accessing embedded files.
    /// </summary>
    EmbeddedFileOpen
}

public class EncryptionDictionary : DictionaryObject
{
    public EncryptionDictionary(DictionaryObject dict) : base(dict)
    {
    }
    
    //The name of the preferred security handler for this document.
    //It shall be the name of the security handler that was used to encrypt the document.
    //If SubFilter is not present, only this security handler shall be used when opening the document.
    //Standard shall be the name of the built-in password-based security handler. However, there may be others in more secure docs.
    public NameObject Filter => GetAs<NameObject>("Filter");

    //Optional- a name that completely specifies the format and interpretation of the contents of the encryption dictionary. 
    //It allows security handlers other than the one specified by Filter to decrypt the document. 
    //If this entry is absent, other security handlers shall not decrypt this document. 
    public NameObject? SubFilter => TryGetAs<NameObject>("SubFilter");
    
    //Required - a code specifying the algorithm to be used in encrypting and decrypting the document.
    //0 - undocumented. Not typically used.
    //1 - Simple encryption, key length of 40 bits.
    //2 - Simple encryption, key length can be greater than 40 bits. 
    //3 - An unpublished algorithm that permits file encryption key lengths ranging from 40 to 128 bits.
    //4 - Uses rules specified in CF, StmF, and StrF entries. with a file encryption key length of 128 bits.
    //5 - Uses rules specified in CF, StmF, and StrF entries. With a file encryption key length of 256 bits.
    [PdfDictionaryKey("V")] public NumericObject AlgorithmCode => GetAs<NumericObject>("V");
    
    //Required - number specifying which revision of the standard security handler shall be used to interpret this dictionary.
    public int Revision => (int)GetAs<NumericObject>("R").Value;
    
    //Required - 32 bytes if R is 4, 48 bytes long if the value of R is 6. Used in computing the file encryption key.
    public StringObject OwnerKey => GetAs<StringObject>("O");
    //Required - same as OKey.
    public StringObject UserKey => GetAs<StringObject>("U");
    
    //Security Keys required if R is 6 (PDF 2.0) used in decryption algorithm.
    public StringObject? OEKey => TryGetAs<StringObject>("OE");
    public StringObject? UEKey => TryGetAs<StringObject>("UE");
    
    //A set of flags specifying which operations shall be permitted when the document is opened with user access.
    //IMPORTANT in determining what you can do with this PDF without a owner password.
    //ONly bit positions that matter are -> 3,4,5,6,9,10,11, and 12.
    public NumericObject PermissionFlags => GetAs<NumericObject>("P");

    //Required if R is 6, a 16-byte string, encrypted with the file encryption key. 
    //Contains a encrypted copy of the permissions flags. 
    public StringObject? EncryptedPermissionFlags => TryGetAs<StringObject>("Perms");

    //Optional - meaningful when V is 4 or 5. Indicates whether the document-level metadata stream shall be encrypted.
    //Default value is true.
    public BooleanObject? EncryptMetadata => TryGetAs<BooleanObject>("EncryptMetadata");
    
    //Optional only if V is 2 or 3. The length of the file encryption key in bits. 
    //The value shall be a multiple of 8, in the range of 40 to 128 bits. Default: 40. 
    public NumericObject? Length => TryGetAs<NumericObject>("Length");
    [PdfDictionaryKey("CF")] public DictionaryObject? CryptFiltersDictionary => TryGetAs<DictionaryObject>("CF"); //meaningful only when the value of V is 4 or 5. 
    [PdfDictionaryKey("StmF")] public NameObject? StreamFilterKey => TryGetAs<NameObject>("StmF"); //meaningful only when the value of V is 4 or 5.
    [PdfDictionaryKey("StrF")] public NameObject? StringFilterKey => TryGetAs<NameObject>("StrF"); //Meaningful only when the value of V is 4 or 5.
    [PdfDictionaryKey("EFF")]
    public NameObject? EmbeddedFilterKey => TryGetAs<NameObject>("EFF"); //Meaningful only when the value of V is 4.
    
    public UserAccessPermissions DocumentPermissions => (UserAccessPermissions)((int)PermissionFlags.Value);
}