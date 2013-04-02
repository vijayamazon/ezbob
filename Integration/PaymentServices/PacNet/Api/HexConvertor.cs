using System;

namespace Raven.API.Support
{
    /**
     * <p>Converts byte arrays to lowercase hexadecimal String equivalents.</p>
     *
     * @author warren
     */
    public class HexConvertor
    {
        /**
         * <p>Converts the supplied array of bytes to a lowercase hexadecimal String equivalent.</p>
         *
         * @param raw the array of bytes to be converted
         *
         * @return a hexadecimal String
         */
        public string GetHex(byte[] raw)
        {
            string hex = null;
            if (raw != null)
            {
                hex = BitConverter.ToString(raw);
                hex = (hex.Replace("-", "")).ToLower();
            }
            return hex;
        }
    }
}
