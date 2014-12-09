using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using Raven.API.Support;

namespace Raven.API
{
    /**
     * <p>A response to the invocation of a Raven operation, as represented by
     * a <code>RavenRequest</code> and associated operation type.</p>
     *
     * @author warren
     */
    public class RavenResponse : RavenSecureAPI
    {
        // The default encoding used for string/byte translations
        private static Encoding defaultEncoding = Encoding.ASCII;

        /** the HTTP status code indicating the relative degree of success of the
         * original request */
        protected HttpStatusCode httpStatusCode;

        /** the Raven request that the receiver is a response to */
        private RavenRequest originalRequest;
        public RavenResponse(){}
        /**
         * <p>Constructs an initialized instance of the receiver using the array
         * of bytes representing the response body of the original request HTTP
         * Post to the Raven server API.</p>
         *
         * <p>The response bytes from Raven will contain multiple key-value parameters,
         * separated by "&", and optionally a sequence of bytes representing
         * comma-separated report data, with each line separated by "\r".
         *
         * @param responseData the array of bytes constituting Raven's response to
         * the original request
         * @param httpStatusCode the HTTP status code indicating the relative degree of
         * success of the original request
         * @param originalRequest the request that the receiver is a response to
         *
         * @throws RavenConfigurationException if the application's default settings do not
         * exist or cannot be found or if any mandatory parameters are missing
         * @throws RavenAuthenticationException if it cannot be verified that the
         * receiver originated with Raven
         */
        public RavenResponse(byte[] responseData, HttpStatusCode httpStatusCode, RavenRequest originalRequest)
        {
            this.httpStatusCode = httpStatusCode;
            this.originalRequest = originalRequest;

            this.paramValuesByKey["HTTPStatus"] = httpStatusCode.ToString();

            if (httpStatusCode == HttpStatusCode.OK)
            {
                this.parseResponse(responseData);
            }
            else
            {
                if (httpStatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new RavenNoResponseException();
                }
            }
        }

        /**
         * <p>Parses the supplied response data.</p>
         *
         * <p>The response bytes from Raven will contain multiple key-value parameters,
         * separated by "&", and optionally a sequence of bytes representing
         * comma-separated report data, with each line separated by "\r".
         *
         * @param responseData the array of bytes constituting Raven's response to
         * the original request
         *
         * @throws RavenAuthenticationException if it cannot be verified that the
         * receiver originated with Raven
         */
        private void parseResponse(byte[] responseData)
        {
            String[] paramAndReportPairs = defaultEncoding.GetString(responseData).Split(new char[] { '\r' }, 2);

            SetResponseParameters(paramAndReportPairs[0]);
            if (paramAndReportPairs.Length == 2)
            {
                SetReportParameters(paramAndReportPairs[1]);
            }

            this.Authenticate();
        }

        /**
         * <p>Sets the receiver's response parameters from the Raven response string
         * comprised of multiple key-value parameters, separated by "&".
         *
         * @param responseParamData a String constituting Raven's response to the
         * original request
         * @throws RavenAuthenticationException if it cannot be verified that the
         * receiver originated with Raven
         */
        private void SetResponseParameters(String responseParamData)
        {
            String[] paramPairs = responseParamData.Split('&');

            for (int i = 0; i < paramPairs.Length; i++)
            {
                String[] keyValue = paramPairs[i].Split('=');
                String value, key;

                if (keyValue.Length > 0)
                {
                    key = keyValue[0];
                    if (keyValue.Length < 2)
                    {
                        value = "";
                    }
                    else
                    {
                        value = HttpUtility.UrlDecode(keyValue[1]);
                    }
                    this.paramValuesByKey[key] = value;
                }
            }
        }

        /**
         * <p>Sets the receiver's report parameters from the Raven response substring
         * (without the request parameters) comprised of multiple key-value
         * parameters, separated by "\r"s.
         *
         * @param reportData a String constituting the report data
         */
        private void SetReportParameters(String reportData)
        {
            this.paramValuesByKey["Report"] = reportData;
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
            return this.GetOriginalOperationType().SignatureBaseRawData(this);
        }

        /**
         * <p>Answers true if the receiver is a valid Raven API interaction.</p>
         *
         * @return <code>true</code> if the receiver is a valid Raven API interaction
         */
        public bool IsAuthentic() {

            try {
                return this.GetSignature().Equals(this.Get("Signature"));
            } catch (RavenIncompleteSignatureException ) {
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

        /**
         * <p>Answers the type of operation that the receiver is in response to.</p>
         *
         * @return the type of operation that the receiver is in response to
         */
        public RavenOperationType GetOriginalOperationType()
        {
            return originalRequest.GetOperationType();
        }
    }
}
