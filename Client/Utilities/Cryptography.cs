using System;
using System.Security.Cryptography;
using System.Text;

namespace AOClient.Utilities
{
    public static class Cryptography
    {
        public static string HashPassword(string passwordToHash)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(passwordToHash);

            byte[] hashedBytes = new SHA256CryptoServiceProvider().ComputeHash(inputBytes);

            return string.Concat(BitConverter.ToString(hashedBytes).Split('-'));
        }
    }
}
