using System;
using System.Collections.Generic;

namespace Raven.API.Support
{
    /**
     * <p>An API to Raven functionality that is primarily driven through the setting
     * of key-value parameter pairs.</p>
     * 
     * <p>This is in contrast to a strongly typed API consisting of specific
     * operation types, each of which is constructed with a well known and therefore
     * relatively fixed subset of parameters.</p>
     *
     * @author warren
     */
    interface KeyValueWritableAPI
    {
        /**
         * <p>Associates the receiver's parameter key with the supplied value.</p>
         * @param paramKey the name of the API parameter being set
         * @param value the value to set
         * @return the value that was set
         */
        String Set(string paramKey, string value);

        /**
         * <p>Replaces the receiver's current parameters with the supplied map
         * of parameter key-value pairs.</p>
         * @param newParamValuesByKey the new parameters to set
         */
        void Reset(Dictionary<String, String> newParamValuesByKey);
    }
}
