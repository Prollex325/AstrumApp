using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AstrumApp.Security.Crypto
{
    internal class HashService
    {
        public HashData Hash(string pin)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            const int iterations = 300_000;

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                pin,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                32);

            HashData result = new HashData
            {
                Hash = Convert.ToBase64String(hash),
                Salt = salt,
                Iterations = iterations
            };
            return result;
        }

        public bool Verify(string pin, HashData hashResult)
        {
            byte[] hashToVerify = Rfc2898DeriveBytes.Pbkdf2(
                pin,
                hashResult.Salt,
                hashResult.Iterations,
                HashAlgorithmName.SHA256,
                32);
            string hashToVerifyBase64 = Convert.ToBase64String(hashToVerify);
            return hashToVerifyBase64 == hashResult.Hash;
        }
    }

    class HashData
    {
        public string Hash { get; set; } = String.Empty;
        public byte[] Salt { get; set; } = Array.Empty<byte>();
        public int Iterations { get; set; } = 300_000;
    }
}
