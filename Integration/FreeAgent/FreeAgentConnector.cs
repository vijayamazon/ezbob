namespace FreeAgent
{
    using System;
    using System.Web.Script.Serialization;
    using EZBob.DatabaseLib.DatabaseWrapper.Order;
    using RestSharp;
    using log4net;

	public class FreeAgentConnector
	{
		// TODO: get from config
		public const string InvoicesRequest = "https://api.freeagent.com/v2/invoices?nested_invoice_items=true";
		
		private static readonly ILog _Log = LogManager.GetLogger(typeof(FreeAgentConnector));

		public static FreeAgentOrdersList GetOrders(FreeAgentSecurityInfo securityInfo, DateTime fromDate)
		{
			var request = new RestRequest(Method.GET) { Resource = InvoicesRequest };
			request.AddHeader("Authorization", "Bearer " + securityInfo.AccessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			var js = new JavaScriptSerializer();
			var invoicesList = (InvoicesList)js.Deserialize(response.Content, typeof(InvoicesList));
			var freeAgentOrdersItemList = new FreeAgentOrdersList(DateTime.UtcNow, invoicesList.Invoices);
			return freeAgentOrdersItemList;
        }
	}
}