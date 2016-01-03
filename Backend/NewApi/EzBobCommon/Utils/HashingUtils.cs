using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon.Utils {
    /// <summary>
    /// Contains various hashing options
    /// </summary>
    public class HashingUtils {
        /// <summary>
        /// The MD5 hash.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string MD5(string input) {
            // step 1, calculate MD5 hash from input
            var md5 = System.Security.Cryptography.MD5.Create();

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input ?? string.Empty);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++) {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString()
                .ToLowerInvariant();
        }


        /// <summary>
        /// Hashes the user name and password by SHA512.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userPassword">The user password.</param>
        /// <returns></returns>
        public static string HashUserNameAndPassword(string userName, string userPassword) {
            return SHA512(userName + userPassword);
        }

        /// <summary>
        /// Hashes the password by SHA512.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public static string HashPassword(string password) {
            return SHA512(password);
        }

        /// <summary>
        /// Hashes the specified password by SHA512.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public static string SHA512(string password)
        {
            var sha = System.Security.Cryptography.SHA512.Create();

            byte[] aryInputBytes = System.Text.Encoding.ASCII.GetBytes(password ?? string.Empty);
            byte[] firstHash = sha.ComputeHash(aryInputBytes);

            // truncating some characters - salt replacement
            byte[] secondHash = sha.ComputeHash(firstHash, 6, firstHash.Length - 14);

            var sb = new StringBuilder();

            for (int i = 0; i < secondHash.Length; i++)
                sb.Append(secondHash[i].ToString("X2"));

            return sb.ToString()
                .ToLowerInvariant();
        }
    }
}
