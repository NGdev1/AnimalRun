using System;
using System.Text;
//using System.IO;
using System.Security.Cryptography;

namespace Assets.Scripts.Common
{
    //public static class Base64
    //{
    //    public static string Encode(string plainText)
    //    {
    //        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);

    //        return Convert.ToBase64String(plainTextBytes);
    //    }

    //    public static string Decode(string base64EncodedData)
    //    {
    //        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);

    //        return Encoding.UTF8.GetString(base64EncodedBytes);
    //    }
    //}

    //public static class AES
    //{
    //    public static int KeyLength = 128;
    //    private const string SaltKey = "ZtP}3zBNF#*Qt7f!fxqqiY8KuX";
    //    //private const string SaltKey = "ShMG8hLyZ7k~Ge5@";
    //    private const string VIKey = "M=11TB0{I7p%+rj!"; // TODO: Generate random VI each encryption and store it with encrypted value

    //    private static string Encrypt(byte[] value, string password)
    //    {
    //        var keyBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(SaltKey)).GetBytes(KeyLength / 8);
    //        var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
    //        var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.UTF8.GetBytes(VIKey));

    //        using (var memoryStream = new MemoryStream())
    //        {
    //            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
    //            {
    //                cryptoStream.Write(value, 0, value.Length);
    //                cryptoStream.FlushFinalBlock();
    //                cryptoStream.Close();
    //                memoryStream.Close();

    //                return Convert.ToBase64String(memoryStream.ToArray());
    //            }
    //        }
    //    }

    //    public static string Encrypt(string value, string password)
    //    {
    //        return Encrypt(Encoding.UTF8.GetBytes(value), password);
    //    }

    //    public static string Decrypt(string value, string password)
    //    {
    //        var cipherTextBytes = Convert.FromBase64String(value);
    //        var keyBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(SaltKey)).GetBytes(KeyLength / 8);
    //        var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC, Padding = PaddingMode.None };
    //        var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.UTF8.GetBytes(VIKey));

    //        using (var memoryStream = new MemoryStream(cipherTextBytes))
    //        {
    //            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
    //            {
    //                var plainTextBytes = new byte[cipherTextBytes.Length];
    //                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

    //                memoryStream.Close();
    //                cryptoStream.Close();

    //                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
    //            }
    //        }
    //    }
    //}

    public class B64X
    {
        public static byte[] Key = Guid.NewGuid().ToByteArray();

        private static string Encode(string value)
        {
            return Convert.ToBase64String(Encode(Encoding.UTF8.GetBytes(value), Key));
        }

        private static string Decode(string value)
        {
            return Encoding.UTF8.GetString(Encode(Convert.FromBase64String(value), Key));
        }

        public static string Encrypt(string value, string key)
        {
            return Convert.ToBase64String(Encode(Encoding.UTF8.GetBytes(value), Encoding.UTF8.GetBytes(key)));
        }

        public static string Decrypt(string value, string key)
        {
            return Encoding.UTF8.GetString(Encode(Convert.FromBase64String(value), Encoding.UTF8.GetBytes(key)));
        }

        private static byte[] Encode(byte[] bytes, byte[] key)
        {
            var j = 0;

            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= key[j];

                if (++j == key.Length)
                {
                    j = 0;
                }
            }

            return bytes;
        }

        public static string GetNewKey()
        {
            byte[] k = Guid.NewGuid().ToByteArray();
            return Convert.ToBase64String(k);
        }
    }

    public static class GooglePlayPurchaseGuard
    {
        /// <summary>
        /// Verify Google Play purchase. Protect you app against hack via Freedom. More info: http://mrtn.me/blog/2012/11/15/checking-google-play-signatures-on-net/
        /// </summary>
        /// <param name="purchaseJson">Purchase JSON string</param>
        /// <param name="base64Signature">Purchase signature string</param>
        /// <param name="xmlPublicKey">XML public key. Use http://superdry.apphb.com/tools/online-rsa-key-converter to convert RSA public key from Developer Console</param>
        /// <returns></returns>
        public static bool Verify(string purchaseJson, string base64Signature, string xmlPublicKey)
        {
            using (var provider = new RSACryptoServiceProvider())
            {
                try
                {
                    provider.FromXmlString(xmlPublicKey);

                    var signature = Convert.FromBase64String(base64Signature);
                    var sha = new SHA1Managed();
                    var data = System.Text.Encoding.UTF8.GetBytes(purchaseJson);

                    return provider.VerifyData(data, sha, signature);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                }

                return false;
            }
        }
    }
}