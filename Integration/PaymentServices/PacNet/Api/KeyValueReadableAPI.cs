using System;
using System.Collections.Generic;

namespace Raven.API.Support
{
    /**
     * <p>An API to Raven functionality that is primarily driven through the use
     * of key-value parameter pairs, but whose values cannot be set directly,
     * only interrogated.</p>
     *
     * <p>This is in contrast to a strongly typed API consisting of specific
     * operation types, each of which is constructed with a well known and therefore
     * relatively fixed subset of parameters.</p>
     *
     * @author warren
     */
    public interface KeyValueReadableAPI
    {
        /**
         * <p>Answers the value associated with the receiver's specified parameter key.</p>
         * @param paramKey the name of the API parameter being retrieved
         * @return the value associated with the key
         */
        string Get(string paramKey);

        /**
         * <p>Answers the receiver's parameter values, mapped by their key names.</p>
         *
         * @return a map of the receiver's parameter values, keyed by their names
         */
        Dictionary<String, String> GetParams();

        /**
         * <p>Answers true if the receiver has a value associated with the specified
         * parameter key.</p>
         * @param paramKey the name of the API parameter being tested
         * @return a boolean, true if the receiver has a value associated with the
         * specified key
         */
        bool IsSet(string paramKey);
    }
}
