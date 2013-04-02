using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.API.Support;

namespace Raven.API
{
    /**
     * <p>An API to Raven functionality that is primarily driven through the
     * use of key-value parameter pairs, but whose values cannot be set directly,
     * only interrogated, with some relatively static parameters able to be read
     * from the application's default settings.</p>
     *
     * @author warren
     */
    public abstract class RavenSecureAPI : KeyValueConfigurableReadableAPI
    {
        /** supplies current timestamps */
        protected static TimestampProvider TimestampProvider = new TimestampProvider();

        /** supplies HMAC signatures to authenticate the receiver to Raven */
        protected SignatureProvider signatureProvider;

        /** the RAPI version being targetted */
        protected String rapiVersion;
        /** the base Raven server URL */
        protected String site;
        /* the client's username */
        protected String userName;
        /** the client's private secret */
        protected String secret;
        protected String prefix;

        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @throws RavenConfigurationException if the application's default settings do not
         * exist or cannot be found or if any mandatory parameters are missing
         */
        public RavenSecureAPI()
        {
            this.rapiVersion = ConfigureParamAsValue("RAPIVersion", "RAVEN_RAPIVERSION", "2", true);
            this.site = ConfigureParam("RAVEN_GATEWAY", "RAVEN_GATEWAY", null, true);
            this.userName = ConfigureParamAsValue("UserName", "RAVEN_USERNAME", null, true);
            this.secret = ConfigureParam("RAVEN_SECRET", "RAVEN_SECRET", null, true);
            this.prefix = ConfigureParam("RAVEN_PREFIX", "RAVEN_PREFIX", null, true);

            try
            {
                signatureProvider = new SignatureProvider(this.secret);
            }
            catch (Exception e)
            {
                throw new RavenConfigurationException("Problem with secret or algorithm.", e);
            }
        }

        /**
         * <p>Answers a signature that guarantees that the receiver is a valid
         * Raven API interaction.</p>
         *
         * @return a signature String
         * @throws RavenIncompleteSignatureException if the signature is incomplete
         * due to one or more missing parameters
         */
        public String GetSignature() {
            return signatureProvider.GetSignature(this.GetSignatureData());
        }

        /**
         * <p>Answers a concatenated string of a subset of the receiver's parameter
         * data values used to determine its signature.</p>
         *
         * @return a concatenated String of a subset of the receiver's parameter
         * data values
         * @throws RavenIncompleteSignatureException if the signature is incomplete
         * due to one or more missing parameters
         */
        protected abstract String GetSignatureData();
    }
}
