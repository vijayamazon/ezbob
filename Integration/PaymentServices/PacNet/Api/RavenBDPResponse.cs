using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.API;

namespace Raven.API.BDP
{
    /**
     * <p>A response to the invocation of a Raven BDP operation.</p>
     *
     * @author warren
     */
    public class RavenBDPResponse : RavenSecureAPI
    {
        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @throws RavenConfigurationException if the the configuration file or any
         * of its mandatory parameters is missing
         */
        public RavenBDPResponse() {
        }

        /**
         * <p>Sets the receiver's API request parameter to the supplied value.</p>
         *
         * @param paramKey the name of the API request parameter being set
         * @param paramValue the value to set
         */
        public void SetParams(Dictionary<String, String[]> newParamValuesByKey)
        {
            foreach (KeyValuePair<string, string[]> keyValuePair in newParamValuesByKey)
            {
                this.paramValuesByKey[keyValuePair.Key] = keyValuePair.Value[0];
            }
        }

        /**
         * <p>Answers a concatenated string of the subset of the receiver's parameter
         * data values used to determine its signature.</p>
         *
         * @return a concatenated String of a subset of the receiver's parameter
         * data values
         * @throws RavenIncompleteSignatureException if the signature is incomplete
         * due to one or more missing parameters
         */
        protected override String GetSignatureData()
        {
            return RavenOperationType.BROWSERDIRECTPOST.SignatureRawData(this);
        }

        /**
         * <p>Answers true if the receiver is a valid Raven API interaction.</p>
         *
         * @return <code>true</code> if the receiver is a valid Raven API interaction
         */
        public bool IsAuthentic()
        {
            try
            {
                return this.GetSignature().Equals(this.Get("Signature"));
            }
            catch (RavenIncompleteSignatureException )
            {
                return false;
            }
        }

        /**
         * <p>Verifies that the receiver is a valid Raven API interaction.</p>
         *
         * @throws RavenAuthenticationException if it cannot be verified that the
         * receiver is a valid Raven API interaction
         */
        protected void Authenticate()
        {
            if (!this.IsAuthentic())
            {
                throw new RavenAuthenticationException("Invalid Raven signature!");
            }
        }
    }
}
