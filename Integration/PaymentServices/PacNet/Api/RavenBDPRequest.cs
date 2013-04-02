using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Raven.API;
using Raven.API.Support;

namespace Raven.API.BDP
{
    /**
     * <p>A request for Raven to perform a Browser Direct Post (BDP) operation on
     * behalf of a specific client.</p>
     *
     * @author warren
     */
    public class RavenBDPRequest : RavenSecureAPI, KeyValueWritableAPI
    {
        /** the receiver is a BDP operation */
        protected RavenOperationType operationType = RavenOperationType.BROWSERDIRECTPOST;
        protected bool isSecured = false;

        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @throws RavenConfigurationException if the the configuration file or any of its
         * mandatory parameters is missing
         */
        public RavenBDPRequest() {

            ConfigureParamAsValue("CallbackURL", "RAVEN_BDP_CALLBACKURL", null, false);
            ConfigureParamAsValue("PaymentType", "RAVEN_BDP_PAYMENTTYPE", "cc_debit", false);
            ConfigureParamAsValue("PaymentRoutingNumber", "RAVEN_BDP_PAYMENTROUTINGNUMBER", null, false);
            this.Set("RAPIInterface", "DotNetBDPVer2.3");
        }

        /**
         * <p>Sets the receiver's API request parameter to the supplied value.</p>
         *
         * @param paramKey the name of the API request parameter being set
         * @param paramValue the value to set
         * @return the new value of the request parameter
         */
        public String Set(string paramKey, string paramValue)
        {
            this.paramValuesByKey[paramKey] = paramValue;
            return paramValue;
        }

        /**
         * <p>Answers the receiver's parameter values, mapped by their key names,
         * augmented with information required to identify and secure the request
         * for the Raven server.</p>
         *
         * @return a map of the receiver's parameter values, keyed by their names
         * @throws RavenIncompleteSignatureException if the signature is incomplete
         * due to one or more missing parameters
         */
        public Dictionary<String, String> GetSecuredParams() {

            if (!this.isSecured) 
            {
                this.paramValuesByKey["RequestID"] = Guid.NewGuid().ToString();
                this.paramValuesByKey["Timestamp"] = TimestampProvider.GetFormattedTimestamp();
                this.paramValuesByKey["Signature"] = this.GetSignature();
                this.isSecured = true;
            }

            return this.paramValuesByKey;
        }

        /**
         * <p>Augments the receiver's operation-specific request parameters with
         * information required to identify and secure the request for the Raven server.</p>
         * 
         * @return a collection of all the request parameter key-value data required
         * by this request, including those required for authentication
         */
        public NameValueCollection GetSecuredPostData()
        {
            NameValueCollection data = new NameValueCollection();

            foreach (KeyValuePair<string, string> keyValuePair in this.GetSecuredParams())
            {
                data.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return data;
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
            return this.operationType.SignatureRawData(this);
        }

        /**
         * <p>Answers the Raven URL used to invoke an operation of the receiver's
         * type.</p>
         *
         * @return the Raven URL used to invoke an operation of the receiver's type
         */
        public string GetOperationURL()
        {
            char last = this.site.ElementAt(this.site.Length - 1);
            if (last == '/')
            {
                return this.site + this.operationType.GetURLParamName();
            }
            else
            {
                return this.site + "/" + this.operationType.GetURLParamName();
            }
        }

        /**
         * <p>Replaces the receiver's current parameters with the supplied map
         * of parameter key-value pairs.</p>
         * @param newParamValuesByKey the new parameters to set
         */
        public void Reset(Dictionary<String, String> newParamValuesByKey)
        {
            this.paramValuesByKey = newParamValuesByKey;
        }
    }
}
