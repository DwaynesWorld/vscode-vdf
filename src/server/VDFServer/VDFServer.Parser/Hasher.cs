using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VDFServer.Parser
{
    public class Hasher
    {
        public static string GetFileHash(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        public static string GetStringHash(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(value))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        private static byte[] GetHash(string value)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
        }
    }
}