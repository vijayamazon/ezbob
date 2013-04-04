using System.Security.Cryptography;
using System.Text;

namespace EzBob.CommonLib.Security
{
    public static class Encryptor
    {
        public static string Encrypt(string input)
        {
            return CipherUtility.Encrypt<AesManaged>(input, GetKey(), "EzBB0b");
        }

        public static byte[] EncryptBytes(string input)
        {
            return Encoding.UTF8.GetBytes(CipherUtility.Encrypt<AesManaged>(input, GetKey(), "EzBB0b"));
        }

        public static string Decrypt(string input)
        {
            return CipherUtility.Decrypt<AesManaged>(input, GetKey(), "EzBB0b");
        }

        public static string Decrypt(byte[] input)
        {
            return CipherUtility.Decrypt<AesManaged>(Encoding.UTF8.GetString(input), GetKey(), "EzBB0b");
        }

        private static string GetKey()
        {
            return "F40D3C355CFD5DC4251D9ADEECC7DD73BFB9D5A80946786F071007399130335D";
        }
    }
}