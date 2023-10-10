namespace Sharpcash.Helpers;

internal static class CryptoHelper
{
    public static HashAlgorithm? CreateHashAlgorithm(HashAlgorithmName hashAlgorithm)
    {
        return hashAlgorithm.Name switch
        {
            "MD5" => MD5.Create(),
            "SHA1" => SHA1.Create(),
            "SHA256" => SHA256.Create(),
            "SHA384" => SHA384.Create(),
            "SHA512" => SHA512.Create(),
            _ => null
        };
    }
}
