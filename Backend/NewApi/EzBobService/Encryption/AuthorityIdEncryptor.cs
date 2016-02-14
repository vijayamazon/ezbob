using System;

namespace EzBobService.Encryption {
    using EzBobCommon.Utils;
    using EzBobCommon.Utils.Encryption;

    internal class AuthorityIdEncryptor {
        private static string[] splitStrings = {
            ":{", "}:{", "}:"
        };

        /// <summary>
        /// Encrypts the customer identifier.
        /// </summary>
        /// <param name="companyId">The customer identifier.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="isDirector">if set to <c>true</c> [is director].</param>
        /// <returns></returns>
        public static string EncryptAuthorityId(int companyId, string origin, bool isDirector) {
            string str = string.Format("3:{{{0}}}:{{{1}}}:{{{2}}}:{{{3}}}:3", companyId, DateTimeUtils.ConvertToIso8601String(DateTime.UtcNow), origin, isDirector ? 1 : 0);
            return EncryptionUtils.SafeEncrypt(str);
        }

        /// <summary>
        /// Decrypts the authority identifier.
        /// </summary>
        /// <param name="authorityId">The authority identifier.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="isDirector">if set to <c>true</c> [is director].</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Invalid authority id.
        /// or
        /// Not a authority id.
        /// or
        /// Origin validation failed.
        /// or
        /// Invalid authority id.
        /// or
        /// Not a director id
        /// or
        /// Not a shareholder id
        /// </exception>
        public static int DecryptAuthorityId(string authorityId, string origin, bool isDirector) {
            DateTime dt;
            return DecryptAuthorityId(authorityId, origin, isDirector, out dt);
        }

        /// <summary>
        /// Decrypts the authority identifier.
        /// </summary>
        /// <param name="authorityId">The authority identifier.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="isDirector">if set to <c>true</c> [is director].</param>
        /// <param name="encryptionTime">The encryption time.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Invalid authority id.
        /// or
        /// Not a authority id.
        /// or
        /// Origin validation failed.
        /// or
        /// Invalid authority id.
        /// or
        /// Not a director id
        /// or
        /// Not a shareholder id
        /// </exception>
        public static int DecryptAuthorityId(string authorityId, string origin, bool isDirector, out DateTime encryptionTime) {
            string str = EncryptionUtils.SafeDecrypt(authorityId);

            var split = str.Split(splitStrings, StringSplitOptions.None);

            if (split.Length != 5) {
                throw new Exception("Invalid authority id " + authorityId);
            }

            if (split[0] != "3" || split[5] != "3") {
                throw new Exception("Not a authority id " + authorityId);
            }

            if (!origin.Equals(split[3], StringComparison.InvariantCultureIgnoreCase)) {
                throw new Exception("Origin validation failed " + authorityId);
            }

            int n = int.Parse(split[4]);
            if (n != 0 || n != 1) {
                throw new Exception("Invalid authority id " + authorityId);
            }

            if (isDirector && n == 0) {
                throw new Exception("Not a director id " + authorityId);
            }

            if (!isDirector && n == 1) {
                throw new Exception("Not a share holder id " + authorityId);
            }

            try {
                encryptionTime = DateTime.Parse(split[2]);
            } catch (Exception ex) {
                throw new Exception("Could not parse date for authority id" + authorityId);
            }

            int res;
            try {
                res = int.Parse(split[1]);
            } catch (Exception ex) {
                throw new Exception("Could no parse authority id " + authorityId);
            }

            return res;
        }
    }
}
