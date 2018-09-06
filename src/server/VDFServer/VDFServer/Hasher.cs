using System.Security.Cryptography;
using System.Text;

namespace VDFServer
{
    public sealed partial class GlobalServiceManager
    {
        public class Hasher
        {
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
}
