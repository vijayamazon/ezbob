using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon.Utils.Encryption {
    using System.IO;
    using System.Security.Cryptography;

    //TODO: copy pasted implementation, review it.
    public static class EncryptionUtils {
        /// <summary>
        /// Encrypts the specified value and returns base64 string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string Encrypt(string value) {
            string password = GetKey();
            string salt = GetSalt();

            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));

            SymmetricAlgorithm algorithm = new AesManaged();

            byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            byte[] rgbIv = rgb.GetBytes(algorithm.BlockSize >> 3);

            ICryptoTransform transform = algorithm.CreateEncryptor(rgbKey, rgbIv);

            using (var buffer = new MemoryStream()) {
                using (var stream = new CryptoStream(buffer, transform, CryptoStreamMode.Write)) {
                    using (var writer = new StreamWriter(stream, Encoding.Unicode)) {
                        writer.Write(value);
                    } // using writer
                } // using stream

                return Convert.ToBase64String(buffer.ToArray());
            } // using buffer
        } // InternalEncrypt

        /// <summary>
        /// Decrypts the text encrypted by <see cref="Encrypt"/>
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string Decrypt(string text) {
            string password = GetKey();
            string salt = GetSalt();

            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));

            SymmetricAlgorithm algorithm = new AesManaged();

            byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            byte[] rgbIv = rgb.GetBytes(algorithm.BlockSize >> 3);

            ICryptoTransform transform = algorithm.CreateDecryptor(rgbKey, rgbIv);

            using (var buffer = new MemoryStream(Convert.FromBase64String(text))) {
                using (var stream = new CryptoStream(buffer, transform, CryptoStreamMode.Read)) {
                    using (var reader = new StreamReader(stream, Encoding.Unicode)) {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        /// <summary>
        /// Encrypts the specified value and returns modified base64 string,
        /// which do not contain problematic characters ('+', '/', '=')
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string SafeEncrypt(string value) {
            string res = Encrypt(value);
            res = res.TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

            return res;
        }

        /// <summary>
        /// Decrypts the string encrypted by <see cref="SafeEncrypt"/>
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string SafeDecrypt(string value) {
            string res = value
                .Replace('_', '/')
                .Replace('-', '+');
            switch (value.Length % 4) {
            case 2:
                res += "==";
                break;
            case 3:
                res += "=";
                break;
            }

            return Decrypt(res);
        }

        private static string GetKey() {
            return "F40D3C355CFD5DC4251D9ADEECC7DD73BFB9D5A80946786F071007399130335D";
        }

        private static string GetSalt() {
            return "EzBB0bE";
        }
    }
}
