namespace EKM
{
    using EKM.API;
    using System;
    using System.Text;
    using System.Collections.Generic;

    /// <summary>
    /// Here we use EKM API classes
    /// The API service reference was generated from http://partnerapi.ekmpowershop1.com/v1.1/partnerapi.asmx
    /// </summary>
    public class EkmConnector
    {
        private static string PartnerKey = "4kNLfm+jv37k0sWb8ojpxGSQ7yx169xz/nS3mmKGiCwUn7fJIl5UxAZthlm44iiEJynebcGHOG/9fJV2/cM4BQ==";
        private static string PartnerEndpointName = "PartnerAPISoap";
        private static string NoErrorIndication = "No data returned";
        private static string LineBreak = "<br/>";

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
                sb.Append("Login to Shop Failed. UserName:").Append(userName);
                errMsg = sb.ToString();
                return false;
            }

            errMsg = string.Empty;
            return true;

        }

        public static ApiKey GetApiKey(string userName, string password)
        {
            // Instantiate Soap Client to access shop data
            var shopClient = new PartnerAPISoapClient();

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

        public static List<Order> GetOrders(string userName, string password)
        {
            var apiKey = EkmConnector.GetApiKey(userName, password);
            var shopClient = new PartnerAPISoapClient(PartnerEndpointName, apiKey.EndPoint);
            var getOrdersRequest = new GetOrdersRequest();

            // Your unique APIKey must be passed with each request
            getOrdersRequest.APIKey = apiKey.Key;
            getOrdersRequest.PartnerKey = PartnerKey;
            getOrdersRequest.FromDate = GetToday(); // "2012-01-01";
            getOrdersRequest.ToDate = GetOneYearBack(); // "2013-02-11";
            var Orders = new List<Order>();
            getOrdersRequest.ItemsPerPage = 100;
            int iPage = 0;
            OrdersObject getOrdersResponse;
            do
            {
                //ReadSalesPage(getOrdersResponse);
                iPage++;
                getOrdersRequest.PageNumber = iPage;
                getOrdersResponse = shopClient.GetOrders(getOrdersRequest);
                if (getOrdersResponse.Orders != null)
                {
                    Orders.AddRange(getOrdersResponse.Orders);
                }
            } while (!IsEndOfOrders(getOrdersResponse));


            //var totalRev = string.Format("Total Store Revenue: {0:###,###,###}", revenue);
            //var totalCount = string.Format("Total Sales #: {0:###,###}", sales);
            //mailText.Append("<h1>New EKM Shop what added for User:").Append(customerId).Append("</h1>");
            //mailText.Append("<h2>Shop Name:").Append(login).Append("</h2>");
            //mailText.Append("<p>").Append(totalRev);
            //mailText.Append("<p>").Append(totalCount);
            //Logger.Info(totalRev);
            //Logger.Info(totalCount);
            return Orders;
        }

        private static string Get00Format(DateTime d)
        {
            return string.Format("{0}-{1}-{2}", d.Year.ToString("00"), d.Month.ToString("00"), d.Day.ToString("00"));
        }

        private static string GetToday()
        {
            return Get00Format(DateTime.Now);
        }

        private static string GetOneYearBack()
        {
            return Get00Format(DateTime.Now.AddYears(-1));
        }

        private static bool IsEndOfOrders(OrdersObject orders)
        {
            // Check if the request failed
            if (orders.Status == StatusCodes.Failure)
            {
                // Output the errors explaining why the request failed
                foreach (var error in orders.Errors)
                {
                    if (error == NoErrorIndication)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /*
        private void ReadSalesPage(OrdersObject orders)
        {
            // Output the shop data (List of shops orders)
            foreach (var order in orders.Orders)
            {
                string orderType = order.OrderStatus;
                if (order.TotalCost.HasValue)
                {
                    try
                    {
                        if (status.ContainsKey(orderType))
                        {
                            status[orderType] += order.TotalCost.Value;
                        }
                        else
                        {
                            status["Other"] += order.TotalCost.Value;
                        }
                    }
                    catch (Exception)
                    {
                        status["Other"] += order.TotalCost.Value;
                    }
                    string monthKey = GetMonthKey(order.OrderDateISO);
                    months[monthKey] = months[monthKey] + order.TotalCost.Value;
                    revenue += order.TotalCost.Value;
                }
                sales += 1;
            }
            Logger.InfoFormat("Page Store Revenue: {0}", revenue);
            Logger.InfoFormat("Page Sales #: {0}", sales);
        }
         */
    }
}