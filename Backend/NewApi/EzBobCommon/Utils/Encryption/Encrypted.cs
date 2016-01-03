using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon.Utils.Encryption
{
    using System.IO;
    using System.Security.Cryptography;

    public class Encrypted
    {
        private readonly string encrypted;

        public static string Decrypt(Encrypted input)
        {
            return input.Decrypt();
        }

        public static string Decrypt(byte[] input)
        {
            return Decrypt(Encoding.UTF8.GetString(input));
        }

        public static string Decrypt(string input)
        {
            return InternalDecrypt(input);
        }

        public static implicit operator byte[](Encrypted obj)
        {
            return Encoding.UTF8.GetBytes(obj.encrypted);
        }

        public static implicit operator string(Encrypted obj)
        {
            return obj.encrypted;
        }

        public Encrypted(string sUnencrypted)
        {
            this.encrypted = InternalEncrypt(sUnencrypted);
        }

        public string Decrypt()
        {
            return Decrypt(this.encrypted);
        }

        public override string ToString()
        {
            return this.encrypted;
        }

        private static string InternalEncrypt(string value)
        {
            string password = GetKey();
            string salt = GetSalt();

            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));

            SymmetricAlgorithm algorithm = new AesManaged();

            byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            byte[] rgbIv = rgb.GetBytes(algorithm.BlockSize >> 3);

            ICryptoTransform transform = algorithm.CreateEncryptor(rgbKey, rgbIv);

            using (var buffer = new MemoryStream())
            {
                using (var stream = new CryptoStream(buffer, transform, CryptoStreamMode.Write))
                {
                    using (var writer = new StreamWriter(stream, Encoding.Unicode))
                    {
                        writer.Write(value);
                    } // using writer
                } // using stream

                return Convert.ToBase64String(buffer.ToArray());
            } // using buffer
        } // InternalEncrypt

        private static string InternalDecrypt(string text)
        {
            string password = GetKey();
            string salt = GetSalt();

            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));

            SymmetricAlgorithm algorithm = new AesManaged();

            byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            byte[] rgbIv = rgb.GetBytes(algorithm.BlockSize >> 3);

            ICryptoTransform transform = algorithm.CreateDecryptor(rgbKey, rgbIv);

            using (var buffer = new MemoryStream(Convert.FromBase64String(text)))
            {
                using (var stream = new CryptoStream(buffer, transform, CryptoStreamMode.Read))
                {
                    using (var reader = new StreamReader(stream, Encoding.Unicode))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        private static string GetKey()
        {
            return "F40D3C355CFD5DC4251D9ADEECC7DD73BFB9D5A80946786F071007399130335D";
        }

        private static string GetSalt()
        {
            return "EzBB0b";
        }
    }
}
