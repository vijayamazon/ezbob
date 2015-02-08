using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MarketplaceWebServiceSellers;
using MarketplaceWebServiceSellers.Model;

namespace AmazonSeller.MarketplaceWebServiceSellers.Model
{
    public class AmazonSecurityInfo
    {
        public AmazonSecurityInfo()
        {
            MarketplaceId = new List<string>();
        }

        public AmazonSecurityInfo(string merchantId, string mwsAuthToken)
        {
            MerchantId = merchantId;
            MWSAuthToken = mwsAuthToken;
        }

        public void AddMarketplace(string marketplaceId)
        {
            MarketplaceId.Add(marketplaceId);
        }

        public string MerchantId { get; set; }
        public string MWSAuthToken { get; set; }
        public List<string> MarketplaceId { get; set; }
    }
    class GetAuthTokens
    {
        private static AmazonSecurityInfo GetAuthToken(string accessKey, string secretKey, string SellerId)
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
            var client = new MarketplaceWebServiceSellersClient(appName, appVersion, accessKey, secretKey, config);

            //Create the request object
            GetAuthTokenRequest req = new GetAuthTokenRequest();
            req.SellerId = SellerId;

            try
            {
                //Try connecting to server to aquire requested data
                GetAuthTokenResponse response = client.GetAuthTokenStatus(req);
                //Create resault objext
                AmazonSecurityInfo result = new AmazonSecurityInfo();
                //Fill in the aquired data to the result object
                result.MWSAuthToken = response.getAuthTokenResult.MWSAuthToken;
                result.MerchantId = response.getAuthTokenResult.SellerId;

                return result;
            }
            catch (Exception e)
            {
                //All exceptions are written to MWSAuthToken field to be handled outside this function
                AmazonSecurityInfo result = new AmazonSecurityInfo();
                result.MWSAuthToken = e.Message;

                return result;
            }
        }

        private static string ReadXMLLine(string accessKey, string secretKey, string input, string id)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(AmazonSecurityInfo));

            using (StringReader reader = new StringReader(input))
            {
                AmazonSecurityInfo info = (AmazonSecurityInfo)(serializer.Deserialize(reader));
                AmazonSecurityInfo result = GetAuthToken(accessKey, secretKey, info.MerchantId);

                if (String.Compare("Request is throttled", result.MWSAuthToken) == 0)
                {
                    Console.WriteLine("Request is throttled, trying again in 60 seconds...");
                    Thread.Sleep(60 * 1000);

                    result = GetAuthToken(accessKey, secretKey, info.MerchantId);

                    if (String.Compare("Request is throttled", result.MWSAuthToken) == 0)
                    {
                        Console.WriteLine("ERROR: Request is throttled");
                        return "";
                    }
                }
                if (String.Compare("Cannot request MWSAuthToken for own account", result.MWSAuthToken) == 0)
                {
                    Console.WriteLine("ERROR: Cannot request MWSAuthToken for own account");
                    return "";
                }

                for (int i = 0; i < info.MarketplaceId.Count; i++)
                    result.AddMarketplace(info.MarketplaceId[i]);

                using (StringWriter writer = new StringWriter())
                {
                    serializer.Serialize(writer, result);
                    return "UPDATE MP_CustomerMarketPlace SET SecurityData=cast('" + writer.ToString() + "' as varbinary(max)) WHERE Id='" + id + "'";
                }
            }
        }//Getting SecurityData in string form and returns an sql-UPDATE string of a single line
    }
}
