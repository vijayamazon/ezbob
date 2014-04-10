namespace Raven.API.Support
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

    /**
     * <p>An API to Raven functionality that is primarily driven through the use
     * of key-value parameter pairs, but whose values cannot be set directly,
     * only interrogated, with some relatively static parameters able to be read
     * from the application's default settings.</p>
     *
     * @author warren
     */
    public abstract class KeyValueConfigurableReadableAPI : KeyValueReadableAPI
    {
        /** the receiver's parameter values, mapped by their associated keys */
        protected Dictionary<String, String> paramValuesByKey = new Dictionary<String, String>();
		
        /**
         * <p>Answers the value associated with the receiver's specified parameter key, or 
         * null if no such parameter is set.</p>
         * 
         * @param paramKey the name of the API parameter being retrieved
         * @return the value associated with the key, or null
         */
        public string Get(string paramKey)
        {
            if (this.paramValuesByKey.ContainsKey(paramKey))
            {
                return this.paramValuesByKey[paramKey];
            }
            else
            {
                return null;
            }
        }

        /**
         * <p>Answers the receiver's parameter values, mapped by their key names.</p>
         *
         * @return a map of the receiver's parameter values, keyed by their names
         */
        public Dictionary<String, String> GetParams()
        {
            return this.paramValuesByKey;
        }

        /**
         * <p>Answers true if the receiver has a value associated with the specified
         * parameter key.</p>
         * @param paramKey the name of the API parameter being tested
         * @return <code>true</code> if the receiver has a value associated with the
         * specified key
         */
        public bool IsSet(string paramKey)
        {
            return this.paramValuesByKey[paramKey] != null;
        }


        /**
         * <p>Answers the number of key-value parameter pairs associated with the
         * receiver.</p>
         *
         * @return the number of key-value parameter pairs associated with the
         * receiver
         */
        protected int Size()
        {
            return this.paramValuesByKey.Count();
        }

        /**
         * <p>Answers the receiver's values in a human readable form.</p>
         *
         * @return a String representing the receiver's values
         */
        public String printValues() 
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<String, String> entry in this.GetParams()) 
            {
                sb.Append(entry.Key);
                sb.Append("=>");
                sb.Append(entry.Value);
                sb.Append("\n");
            }
            return sb.ToString();
        }
    }
}
