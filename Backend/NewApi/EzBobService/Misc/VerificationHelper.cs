namespace EzBobService.Misc {
    using System;
    using EzBobCommon.Utils;
    using EzBobCommon.Utils.Encryption;

    /// <summary>
    /// Generates verification word, encrypts it and validates
    /// </summary>
    public class VerificationHelper {
        /// <summary>
        /// Generates the verification word.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public string GenerateVerificationWord(int length = 5) {
            return StringUtils.GenerateRandomEnglishString(length);
        }

        /// <summary>
        /// Encrypts the verification word.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public string EncryptVerificationWord(string word) {
            return EncryptionUtils.SafeEncrypt(word);
        }

        /// <summary>
        /// Validates the specified word.
        /// </summary>
        /// <param name="wordToValidate">The word to validate.</param>
        /// <param name="encryptedWord">The encrypted word.</param>
        /// <returns></returns>
        public bool Validate(string wordToValidate, string encryptedWord) {

            string decrypted = EncryptionUtils.SafeDecrypt(encryptedWord);
            return wordToValidate.Equals(decrypted, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
