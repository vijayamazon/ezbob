using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using Raven.API.Support;

namespace Raven.API
{
    /**
     * <p>A request for Raven API functionality.</p>
     *
     * <p>The request is parameterized through the use of key-value parameter pairs.
     * The parameter values can be set directly or read from the Raven application's 
     * default settings.</p>
     *
     * <p>The exact operation being requested is indicated by the receiver's
     * operation type, whose values are restricted according to the Raven
     * interface and its defined enumeration of such operation types.</p>
     *
     * @author warren
     */
    public class RavenRequest : RavenSecureAPI, KeyValueWritableAPI
    {
        /** the type of operation (e.g. submit) */
        protected RavenOperationType operationType;

        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @param operationName the name of the Raven operation to be performed
         *
         * @throws RavenNoSuchOperationException if the operation is unknown to Raven
         * @throws RavenConfigurationException if the application's default settings do not
         * exist or cannot be found or if any mandatory parameters are missing
         */
        public RavenRequest(string operationName)
        {
            try
            {
                this.operationType = (RavenOperationType)Enum.Parse(typeof(RavenOperationType), operationName, true);
                this.Set("RAPIInterface","DotNetVer2.3");
            }
            catch (Exception )
            {
                throw new RavenNoSuchOperationException(operationName);
            }
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
         * <p>Performs this request by sending its API request parameters to the
         * Raven server.</p>
         *
         * <p>All the receiver's operation-specific parameter key-value pairs are
         * placed in an HTTP post, along with some additional authentication
         * information such as the client username, which is then sent to Raven.</p>
         *
         * @return a <code>RavenResponse</code>
         *
         * @throws RavenNoResponseException if no response was received
         * @throws RavenAuthenticationException if the response was received but could
         * not be verified to have originated with Raven
         */
        public RavenResponse Send()
        {
            try
            {
                WebClient client = new WebClient();
                byte[] responseData = client.UploadValues(this.GetOperationURL(), "POST", this.SecuredPostData());
                return new RavenResponse(responseData, HttpStatusCode.OK, this);

            }
            catch (WebException we)
            {
                if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    return new RavenResponse(new byte[] { }, ((HttpWebResponse)we.Response).StatusCode, this);
                }
                else
                {
                    throw new RavenNoResponseException(we);
                }
            }
        }

        /**
         * <p>Augments the receiver's operation-specific request parameters with
         * information required to identify and secure the request for the Raven server.</p>
         * 
         * @return a collection of all the request parameter key-value data required
         * by this request, including those required for authentication
         */
        protected NameValueCollection SecuredPostData()
        {
            if (!this.paramValuesByKey.ContainsKey("RequestID")) 
            {
                this.paramValuesByKey["RequestID"] = Guid.NewGuid().ToString();
            }

            this.paramValuesByKey["Timestamp"] = TimestampProvider.GetFormattedTimestamp();
            this.paramValuesByKey["Signature"] = this.GetSignature();

            NameValueCollection data = new NameValueCollection();

            foreach (KeyValuePair<string, string> keyValuePair in this.paramValuesByKey)
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
        public void Reset(Dictionary<String,String> newParamValuesByKey)
        {
            this.paramValuesByKey = newParamValuesByKey;
        }

        /**
         * <p>Resets the receiver's request ID, essentially making it new/unique again.</p>
         */
        public void Reset()
        {
            this.paramValuesByKey["RequestID"] = Guid.NewGuid().ToString();
        }

        /**
         * <p>Answers the type of operation that the receiver represents.</p>
         *
         * @return the type of operation that the receiver represents
         */
        public RavenOperationType GetOperationType()
        {
            return this.operationType;
        }
    }
}
