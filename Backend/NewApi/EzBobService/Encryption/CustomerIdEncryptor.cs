using System;

namespace EzBobService.Encryption {
    using EzBobCommon.Utils;
    using EzBobCommon.Utils.Encryption;

    internal static class CustomerIdEncryptor {
        private static string[] splitStrings = {
            ":{", "}:{", "}:"
        };

        /// <summary>
        /// Encrypts the customer identifier.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        public static string EncryptCustomerId(int customerId, string origin) {
            string str = string.Format("1:{{{0}}}:{{{1}}}:{{{2}}}:1", customerId, DateTimeUtils.ConvertToIso8601String(DateTime.UtcNow), origin);
            return EncryptionUtils.SafeEncrypt(str);
        }

        /// <summary>
        /// Decrypts the customer identifier.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Invalid customer id
        /// or
        /// Not a customer id
        /// or
        /// Origin validation failed
        /// </exception>
        public static int DecryptCustomerId(string customerId, string origin) {
            DateTime dt;
            return DecryptCustomerId(customerId, origin, out dt);
        }

        /// <summary>
        /// Decrypts the customer identifier.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="encryptionTime">The encryption time.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Invalid customer id
        /// or
        /// Not a customer id
        /// or
        /// Origin validation failed
        /// </exception>
        public static int DecryptCustomerId(string customerId, string origin, out DateTime encryptionTime) {

            string str = EncryptionUtils.SafeDecrypt(customerId);

            var split = str.Split(splitStrings, StringSplitOptions.None);

            if (split.Length != 5) {
                throw new Exception("Invalid customer id " + customerId);
            }

            if (split[0] != "1" || split[4] != "1") {
                throw new Exception("Not a customer id " + customerId);
            }

            if (!origin.Equals(split[3], StringComparison.InvariantCultureIgnoreCase)) {
                throw new Exception("Origin validation failed for customer id " + customerId);
            }

            try {
                encryptionTime = DateTime.Parse(split[2]);
            } catch (Exception ex) {
                throw new Exception("Could not parse date for customer id" + customerId);
            }

            int res;
            try {
                res = int.Parse(split[1]);
            } catch (Exception ex) {
                throw new Exception("Could no parse customer id " + customerId);
            }

            return res;
        }
    }
}

