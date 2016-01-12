using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobAcceptanceTests.Infra.Utils {
    public static class Generator {
        private static readonly string Alphabet = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string PasswordAlphaBet = "abcdefghijklmnopqrstuvwxyzABCDFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$;+";

        private static readonly string[] TLDs;
        private static readonly Random Random = new Random();

        static Generator() {
            string tlds = EzBobAcceptanceTests.Properties.Resources.tlds_alpha_by_domain;
            TLDs = tlds.Split(' ', '\n');
        }

        /// <summary>
        /// Gets the random password.
        /// </summary>
        /// <param name="passwordlLength">Length of the password.</param>
        /// <returns></returns>
        public static string GetRandomPassword(uint passwordlLength = 8) {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < passwordlLength; ++i) {
                int idx = Random.Next(0, PasswordAlphaBet.Length - 1);
                builder.Append(PasswordAlphaBet[idx]);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets the random integer.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <returns></returns>
        public static int GetRundomInteger(int minValue, int maxValue) {
            return Random.Next(minValue, maxValue);
        }

        /// <summary>
        /// Gets the random email address.
        /// </summary>
        /// <returns></returns>
        public static string GetRandomEmailAddress() {
            StringBuilder builder = new StringBuilder();
            GenerateName(7, 3, builder);
            builder.Append("@");
            GenerateName(5, 0, builder);
            builder.Append(".");

            int tldIdx = Random.Next(0, TLDs.Length - 1);
            builder.Append(TLDs[tldIdx].Trim());
            return builder.ToString();
        }

        /// <summary>
        /// Generates the name.
        /// </summary>
        /// <param name="numberOfChars">The number of chars.</param>
        /// <param name="numberOfDigits">The number of digits.</param>
        /// <param name="builder">The builder.</param>
        private static void GenerateName(uint numberOfChars, uint numberOfDigits, StringBuilder builder) {
            int alphaBetLength = Alphabet.Length;

            for (int i = 0; i < Math.Max(1, numberOfChars); ++i) {
                int idx = Random.Next(0, alphaBetLength - 1);
                builder.Append(Alphabet[idx]);
            }

            for (int i = 0; i < numberOfDigits; ++i) {
                builder.Append(Random.Next(0, 9));
            }
        }
    }
}
