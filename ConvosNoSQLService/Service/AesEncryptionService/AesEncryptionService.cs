using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Service.AesEncryptionService
{
    public class AesEncryptionService : IAesEncryptionService
    {
        private readonly byte[] _key;
        public AesEncryptionService()
        {
            string base64Key = Environment.GetEnvironmentVariable("AES_ENCRYPTION_KEY")?.Trim();
            Console.WriteLine("start--" + base64Key + "--end");

            if (string.IsNullOrEmpty(base64Key))
            {
                throw new Exception("AES encryption key is missing in environment variables.");
            }

            try
            {
                _key = Convert.FromBase64String(base64Key);
            }
            catch (FormatException ex)
            {
                throw new Exception("Failed to convert AES encryption key from Base64.", ex);
            }

            if (_key.Length != 32)
            {
                throw new Exception("The AES encryption key must be 32 bytes for AES-256.");
            }
        }

        public string Encrypt(string plaintext)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV();

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plaintext);
                        }
                    }
                    var iv = aes.IV;
                    var encrypted = ms.ToArray();
                    var result = new byte[iv.Length + encrypted.Length];

                    Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                    Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

                    return Convert.ToBase64String(result);
                }
            }
        }

        public string Decrypt(string ciphertext)
        {
            var fullCipher = Convert.FromBase64String(ciphertext);

            using (var aes = Aes.Create())
            {
                aes.Key = _key;

                var iv = new byte[aes.BlockSize / 8];
                var cipher = new byte[fullCipher.Length - iv.Length];

                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                aes.IV = iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream(cipher))
                {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (var sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}