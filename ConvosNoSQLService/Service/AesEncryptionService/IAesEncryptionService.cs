using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.AesEncryptionService
{
    public interface IAesEncryptionService
    {
        string Encrypt(string plaintext);
        string Decrypt(string ciphertext);
    }
}