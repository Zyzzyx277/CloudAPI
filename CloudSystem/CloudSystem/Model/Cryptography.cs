using System.Security.Cryptography;
using System.Text;

namespace CloudSystem.Model;

public class Cryptography
{
    public static string Encrypt(string textToEncrypt, string publicKeyString)
    {
        var bytesToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);

        using var rsa = new RSACryptoServiceProvider(2048);
        try
        {
            rsa.ImportFromPem(publicKeyString);
            var encryptedData = rsa.Encrypt(bytesToEncrypt, true);
            var base64Encrypted = Convert.ToBase64String(encryptedData);
            return base64Encrypted;
        }
        finally
        {
            rsa.PersistKeyInCsp = false;
        }
    }
    public static string Decrypt(string textToDecrypt, string privateKeyString)
    {

        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            try
            {

                // server decrypting data with private key                    
                rsa.ImportFromPem(privateKeyString);

                var resultBytes = Convert.FromBase64String(textToDecrypt);
                var decryptedBytes = rsa.Decrypt(resultBytes, true);
                var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                return decryptedData.ToString();
            }
            finally
            {
                rsa.PersistKeyInCsp = false;
            }
        }
    }
    
}