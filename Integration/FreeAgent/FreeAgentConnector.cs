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
		public const string ExpensesRequest = "https://api.freeagent.com/v2/expenses";
		public const string ExpensesRequestDatePart = "?from_date={0}-{1}-{2}";
		
		private static readonly ILog _Log = LogManager.GetLogger(typeof(FreeAgentConnector));

		public static FreeAgentInvoicesList GetInvoices(string accessToken, int numOfMonths)
		{
			string monthPart = numOfMonths == -1 ? string.Empty : string.Format(InvoicesRequestMonthPart, numOfMonths);
			string timedInvoicesRequest = string.Format("{0}{1}", InvoicesRequest, monthPart);
			var request = new RestRequest(Method.GET) { Resource = timedInvoicesRequest };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			var js = new JavaScriptSerializer();
			var invoicesList = (InvoicesListHelper)js.Deserialize(response.Content, typeof(InvoicesListHelper));
			var freeAgentInvoicesList = new FreeAgentInvoicesList(DateTime.UtcNow, invoicesList.Invoices);
			return freeAgentInvoicesList;
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

		public static FreeAgentExpensesList GetExpenses(string accessToken, DateTime? fromDate)
		{
			string fromDatePart = fromDate == null ? string.Empty : string.Format(ExpensesRequestDatePart, fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day);
			string expensesRequest = string.Format("{0}{1}", ExpensesRequest, fromDatePart);
			var request = new RestRequest(Method.GET) { Resource = expensesRequest };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			var js = new JavaScriptSerializer();
			var expensesList = (ExpensesListHelper)js.Deserialize(response.Content, typeof(ExpensesListHelper));
			var freeAgentExpenesList = new FreeAgentExpensesList(DateTime.UtcNow, expensesList.Expenses);
			return freeAgentExpenesList;
		}
	}
}