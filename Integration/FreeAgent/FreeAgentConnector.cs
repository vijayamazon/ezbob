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
		public const string InvoicesRequestMonthPart = "&view=last_{0}_months";
		public const string CompanyRequest = "https://api.freeagent.com/v2/company";
		public const string UsersRequest = "https://api.freeagent.com/v2/users";
		
		private static readonly ILog _Log = LogManager.GetLogger(typeof(FreeAgentConnector));

		public static FreeAgentOrdersList GetOrders(string accessToken, int numOfMonths)
		{
			string monthPart = numOfMonths == -1 ? string.Empty : string.Format(InvoicesRequestMonthPart, numOfMonths);
			string timedInvoicesRequest = string.Format("{0}{1}", InvoicesRequest, monthPart);
			var request = new RestRequest(Method.GET) { Resource = timedInvoicesRequest };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			var js = new JavaScriptSerializer();
			var invoicesList = (InvoicesList)js.Deserialize(response.Content, typeof(InvoicesList));
			var freeAgentOrdersItemList = new FreeAgentOrdersList(DateTime.UtcNow, invoicesList.Invoices);
			return freeAgentOrdersItemList;
        }

		public static FreeAgentCompany GetCompany(string accessToken)
		{
			var request = new RestRequest(Method.GET) { Resource = CompanyRequest };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			var js = new JavaScriptSerializer();
			var freeAgentCompany = (FreeAgentCompanyList)js.Deserialize(response.Content, typeof(FreeAgentCompanyList));
			return freeAgentCompany.Company;
		}

		public static FreeAgentUsersList GetUsers(string accessToken)
		{
			var request = new RestRequest(Method.GET) { Resource = UsersRequest };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			var js = new JavaScriptSerializer();

			var freeAgentUsers = (FreeAgentUsersList)js.Deserialize(response.Content, typeof(FreeAgentUsersList));
			return freeAgentUsers;
		}
	}
}