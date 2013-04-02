using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raven.API.BDP
{
    /**
     * <p>The intermediate response to the invocation of a Raven BDP operation, used
     * to handle the result of a Raven authorization attempt and eventually return
     * a response to the original client Raven BDP request.</p>
     *
     * @author warren
     */
    public class RavenBDPAuthorization : RavenBDPResponse
    {
        /** the URL to be eventually displayed to the client browser */
        protected String resultURL;

        /**
         * <p>Constructs an initialized instance of the receiver.</p>
         *
         * @throws RavenConfigurationException if the application's default settings do not
         * exist or cannot be found or if any mandatory parameters are missing
         */
        public RavenBDPAuthorization() {
            this.resultURL = ConfigureParamAsValue("ResultURL", "RAVEN_BDP_RESULTURL", null, false);
        }

        /**
         * <p>Answers the URL to be eventually displayed to the client browser,
         * including all of the receiver's parameter values.</p>
         *
         * @return a URL String which includes all of the receiver's parameter values
         */
        public String GetResultURL()
        {
            String baseResultURL = this.Get("ResultURL") + "?";

            foreach (KeyValuePair<string, string> nextKeyValuePair in this.paramValuesByKey)
            {
                baseResultURL = baseResultURL + nextKeyValuePair.Key + "" + nextKeyValuePair.Value + "&";
            }

            return baseResultURL;
        }
    }
}
