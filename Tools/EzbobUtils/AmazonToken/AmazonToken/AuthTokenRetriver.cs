using System;
using MarketplaceWebServiceSellers;
using MarketplaceWebServiceSellers.Model;

namespace AmazonToken
{
    public class AuthTokenRetriver
    {
        public string MWSAuthToken { get; set; }
        public string error;
        public bool doRetry;
        private const string AccessKey = "AKIAJXUDX6A3XIMZLWFA";
        private const string SecretKey = "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";

        public AuthTokenRetriver()
        {
            MWSAuthToken = "";
            error = "";
            doRetry = true;
        }

        public AuthTokenRetriver GetAuthToken(string SellerId)
        {
            // The client application name
            string appName = "C#";

            // The client application version
            string appVersion = "4.0";

            // The endpoint for region service and version (see developer guide)
            string serviceURL = "https://mws.amazonservices.co.uk";

            // Create a configuration object
            MarketplaceWebServiceSellersConfig config = new MarketplaceWebServiceSellersConfig();
            config.ServiceURL = serviceURL;

            // Create the client itself
            var client = new MarketplaceWebServiceSellersClient(appName, appVersion, AccessKey, SecretKey, config);

            //Create the request object
            GetAuthTokenRequest req = new GetAuthTokenRequest();
            req.SellerId = SellerId;

            try
            {
                //Try connecting to server to aquire requested data
                GetAuthTokenResponse response = client.GetAuthTokenStatus(req);
                doRetry = false;
                this.MWSAuthToken=response.getAuthTokenResult.MWSAuthToken;
                return this;
            }
            catch (Exception e)
            {
                if (e.Message != "Request is throttled")
                    doRetry = false;
                error= e.Message;
                return this;
            }
        }
    }
}
