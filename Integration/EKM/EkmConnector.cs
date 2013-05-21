namespace EKM
{
    using API;
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.ServiceModel;
    using log4net;

	/// <summary>
    /// Here we use EKM API classes
    /// The API service reference was generated from http://partnerapi.ekmpowershop1.com/v1.1/partnerapi.asmx
    /// </summary>
    public class EkmConnector
	{
		private static readonly ILog _Log = LogManager.GetLogger(typeof(EkmConnector));
        private static string PartnerKey = "4kNLfm+jv37k0sWb8ojpxGSQ7yx169xz/nS3mmKGiCwUn7fJIl5UxAZthlm44iiEJynebcGHOG/9fJV2/cM4BQ==";
        private static string PartnerEndpointName = "PartnerAPISoap";
        private static string PartnerContractName = "API.PartnerAPISoap";
        private static string NoErrorIndication = "No data returned";
        private static string LineBreak = "<br />";

        public bool Validate(string userName, string password, out string errMsg)
        {
            var apiKey = GetApiKey(userName, password);
            if (apiKey.Status == StatusCodes.Failure)
            {
                // Combine the errors explaining why the request failed
                StringBuilder sb = new StringBuilder();
                int counter = 1;
                foreach (var error in apiKey.Errors)
                {
                    if (counter != 1)
                    {
                        sb.Append(LineBreak);
                    }
                    sb.Append(error);
                    counter++;
                }

                if (counter != 1)
                {
                    sb.Append(LineBreak);
				}
				_Log.Info(sb.ToString());
                errMsg = "Invalid Credentials";
                return false;
            }

            errMsg = string.Empty;
            return true;
        }

        public static ApiKey GetApiKey(string userName, string password)
        {
            var myBinding = new BasicHttpBinding() { Name = PartnerEndpointName };
            var myEndpoint = new EndpointAddress("http://partnerapi.ekmpowershop1.com/v1.1/partnerapi.asmx");
            //// Instantiate Soap Client to access shop data
            var shopClient = new PartnerAPISoapClient(myBinding, myEndpoint);
            shopClient.Endpoint.Binding = myBinding;
            shopClient.Endpoint.Contract.Name = PartnerContractName;
            shopClient.Endpoint.Name = PartnerEndpointName;
         
            // Form request to retrieve shop data (Shop details)
            var getKeyRequest = new GetKeyRequest();

            // Your unique PartnerKey must be passed with each request
            getKeyRequest.PartnerKey = PartnerKey;

            // The customers ekmPowershop username
            getKeyRequest.UserName = userName;

            // The customers ekmPowershop password
            getKeyRequest.Password = password;

            // Retrieve shop data (Shop details)
            var getKeyResponse = shopClient.GetKey(getKeyRequest);

            return getKeyResponse;
        }

        public static List<Order> GetOrders(string userName, string password, DateTime fromDate)
        {
            var apiKey = EkmConnector.GetApiKey(userName, password);
            var myBinding = new BasicHttpBinding() { Name = PartnerEndpointName };
            var shopClient = new PartnerAPISoapClient(myBinding, new EndpointAddress(apiKey.EndPoint));
            
            var getOrdersRequest = new GetOrdersRequest();

            // Your unique APIKey must be passed with each request
            getOrdersRequest.APIKey = apiKey.Key;
            getOrdersRequest.PartnerKey = PartnerKey;
            getOrdersRequest.ToDate = GetToday();
	        getOrdersRequest.FromDate = Get00Format(fromDate);
            var Orders = new List<Order>();
            getOrdersRequest.ItemsPerPage = 100;
            int iPage = 0;
            OrdersObject getOrdersResponse;
            do
            {
                iPage++;
                getOrdersRequest.PageNumber = iPage;
                getOrdersResponse = shopClient.GetOrders(getOrdersRequest);
                if (getOrdersResponse.Orders != null)
                {
                    Orders.AddRange(getOrdersResponse.Orders);
                }
            } while (getOrdersResponse.Status != StatusCodes.Failure);

            return Orders;
        }

        private static string Get00Format(DateTime d) {
	        return d.ToString("yyyy-MM-dd");
        }

        private static string GetToday()
        {
            return Get00Format(DateTime.Now);
        }
    }
}