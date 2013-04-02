using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Raven.API.Support
{
    /**
     * <p>Supplies hexadecimal, hash-based Message Authentication Code (HMAC)
     * signatures that effectively sign strings of data using secret keys.</p>
     *
     * @author warren
     */
    public class SignatureProvider
    {
        // The default encoding used for string/byte translations
        private static Encoding defaultEncoding = Encoding.ASCII;

        // Provides HMACs
        private HMACSHA1 hmacsha;

        // Converts HMAC byte arrays to hex
        protected HexConvertor hexConvertor;

        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @param secret the secret
         */
        public SignatureProvider(string secret)
        {
            hexConvertor = new HexConvertor();
            hmacsha = new System.Security.Cryptography.HMACSHA1();
            hmacsha.Key = (defaultEncoding.GetBytes(secret));
        }

        /**
         * <p>
         * Answers a hexadecimal, hash-based Message Authentication Code (HMAC)
         * signature that effectively signs the supplied data using the configured
         * secret.
         * </p>
         *
         * @param dataToSign the data string to sign
         * @return a hexadecimal HMAC
         */
        public string GetSignature(String dataToSign)
        {
            return this.hexConvertor.GetHex(hmacsha.ComputeHash(defaultEncoding.GetBytes(dataToSign)));
        }
    }
}
