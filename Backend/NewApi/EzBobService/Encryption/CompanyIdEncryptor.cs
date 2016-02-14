using System;

namespace EzBobService.Encryption {
    using EzBobCommon.Utils;
    using EzBobCommon.Utils.Encryption;

    internal static class CompanyIdEncryptor {
        private static string[] splitStrings = {
            ":{", "}:{", "}:"
        };

        /// <summary>
        /// Encrypts the customer identifier.
        /// </summary>
        /// <param name="companyId">The customer identifier.</param>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        public static string EncryptCompanyId(int companyId, string origin) {
            string str = string.Format("2:{{{0}}}:{{{1}}}:{{{2}}}:2", companyId, DateTimeUtils.ConvertToIso8601String(DateTime.UtcNow), origin);
            return EncryptionUtils.SafeEncrypt(str);
        }

        /// <summary>
        /// Decrypts the company identifier.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Invalid company id.
        /// or
        /// Not a company id.
        /// or
        /// Origin validation failed
        /// </exception>
        public static int DecryptCompanyId(string companyId, string origin) {
            DateTime dt;
            return DecryptCompanyId(companyId, origin, out dt);
        }

        /// <summary>
        /// Decrypts the company identifier.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="encryptionTime">The encryption time.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Invalid company id.
        /// or
        /// Not a company id.
        /// or
        /// Origin validation failed
        /// </exception>
        public static int DecryptCompanyId(string companyId, string origin, out DateTime encryptionTime) {
            string str = EncryptionUtils.SafeDecrypt(companyId);

            var split = str.Split(splitStrings, StringSplitOptions.None);

            if (split.Length != 5) {
                throw new Exception("Invalid company id " + companyId);
            }

            if (split[0] != "2" || split[4] != "2") {
                throw new Exception("Not a company id " + companyId);
            }

            if (!origin.Equals(split[3], StringComparison.InvariantCultureIgnoreCase)) {
                throw new Exception("Origin validation failed for company id " + companyId);
            }

            try {
                encryptionTime = DateTime.Parse(split[2]);
            } catch (Exception ex) {
                throw new Exception("Could not parse date for company id" + companyId);
            }

            int res;
            try {
                res = int.Parse(split[1]);
            } catch (Exception ex) {
                throw new Exception("Could no parse company id " + companyId);
            }

            return res;
        }
    }
}
