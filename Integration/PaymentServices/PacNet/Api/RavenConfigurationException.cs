using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.API
{
    /**
     * <p>Raven is not properly configured, probably due to a missing configuration
     * file or absent mandatory configuration parameter.</p>
     *
     * @author warren
     */
    [Serializable()]
    public class RavenConfigurationException : RavenException
    {
        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @param missingConfigParamName the name of the missing configuration parameter
         */
        public RavenConfigurationException(string missingConfigParamName) : base("Raven " + missingConfigParamName + " must be defined in the Raven's property settings in RAPILibv2.2.dll.config.") { }

        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @param originalException the exception that prevented the configuration file from being read
         */
        public RavenConfigurationException(System.Exception originalException) : base("Error reading RAPILibv2.2.dll.config.", originalException) { }

        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @param message the explanation of the error condition
         * @param originalException the exception that prevented the configuration file from being read
         */
        public RavenConfigurationException(String message, System.Exception originalException) : base("Error reading RAPILibv2.2.dll.config.", originalException) { }

        // Constructor needed for serialization when exception propagates from a remoting server to the client. 
        protected RavenConfigurationException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
