namespace FreeAgent
{
    using System;
    using System.IO;
    using System.Net;
    using System.Web.Script.Serialization;
    using Config;
    using EZBob.DatabaseLib.DatabaseWrapper.Order;
    using RestSharp;
	using log4net;
	using StructureMap;

	public class FreeAgentConnector
	{
		private static readonly IFreeAgentConfig config = ObjectFactory.GetInstance<IFreeAgentConfig>();
		private static readonly ILog log = LogManager.GetLogger(typeof(FreeAgentConnector));

		public static FreeAgentInvoicesList GetInvoices(string accessToken, int numOfMonths)
		{
			string monthPart = numOfMonths == -1 ? string.Empty : string.Format(config.InvoicesRequestMonthPart, numOfMonths);
			string timedInvoicesRequest = string.Format("{0}{1}", config.InvoicesRequest, monthPart);
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
			var request = new RestRequest(Method.GET) { Resource = config.CompanyRequest };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			var js = new JavaScriptSerializer();
			var freeAgentCompany = (FreeAgentCompanyList)js.Deserialize(response.Content, typeof(FreeAgentCompanyList));
			return freeAgentCompany.Company;
		}

		public static FreeAgentUsersList GetUsers(string accessToken)
		{
			var request = new RestRequest(Method.GET) { Resource = config.UsersRequest };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			var js = new JavaScriptSerializer();

			var freeAgentUsers = (FreeAgentUsersList)js.Deserialize(response.Content, typeof(FreeAgentUsersList));
			return freeAgentUsers;
		}

		public static FreeAgentExpensesList GetExpenses(string accessToken, DateTime? fromDate)
		{
			string fromDatePart = fromDate == null ? string.Empty : string.Format(config.ExpensesRequestDatePart, fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day);
			string expensesRequest = string.Format("{0}{1}", config.ExpensesRequest, fromDatePart);
			var request = new RestRequest(Method.GET) { Resource = expensesRequest };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			var js = new JavaScriptSerializer();
			var expensesList = (ExpensesListHelper)js.Deserialize(response.Content, typeof(ExpensesListHelper));
			var freeAgentExpenesList = new FreeAgentExpensesList(DateTime.UtcNow, expensesList.Expenses);
			return freeAgentExpenesList;
		}
		
		public static AccessTokenContainer GetToken(string code, string redirectVal, out string errorMessage)
		{
			string accessTokenRequest =
				string.Format("{0}?grant_type=authorization_code&code={1}&redirect_uri={2}&scope=&client_secret={3}&client_id={4}",
							  config.OAuthTokenEndpoint, code, redirectVal, config.OAuthSecret,
							  config.OAuthIdentifier);
			var request = (HttpWebRequest)WebRequest.Create(accessTokenRequest);
			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = "POST";
			errorMessage = null;
			try
			{
				using (var twitpicResponse = (HttpWebResponse)request.GetResponse())
				{
					var response = twitpicResponse.GetResponseStream();
					if (response != null)
					{
						using (var reader = new StreamReader(response))
						{
							var js = new JavaScriptSerializer();
							var objText = reader.ReadToEnd();
							return (AccessTokenContainer)js.Deserialize(objText, typeof(AccessTokenContainer));
						}
					}
				}
			}
			catch (Exception e)
			{
				errorMessage = "Failure getting access token";
				log.WarnFormat("{0}. Exception:{1}", errorMessage, e);
			}
			return null;
		}

		public static AccessTokenContainer RefreshToken(string refreshToken)
		{
			string accessTokenRequest = string.Format("{0}?grant_type=refresh_token&refresh_token={1}&client_secret={2}&client_id={3}",
				config.OAuthTokenEndpoint, refreshToken, config.OAuthSecret, config.OAuthIdentifier);
			var request = (HttpWebRequest)WebRequest.Create(accessTokenRequest);
			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = "POST";
			try
			{
				using (var twitpicResponse = (HttpWebResponse)request.GetResponse())
				{
					var response = twitpicResponse.GetResponseStream();
					if (response != null)
					{
						using (var reader = new StreamReader(response))
						{
							var js = new JavaScriptSerializer();
							var objText = reader.ReadToEnd();
							var a = (AccessTokenContainer)js.Deserialize(objText, typeof(AccessTokenContainer));
							a.refresh_token = refreshToken;
							return a;
						}
					}
				}
			}
			catch (Exception e)
			{
				log.WarnFormat("Failed refreshing token. Exception:{0}", e);
			}

			return null;
		}

		public static string GetApprovalRequest(string redirectUri)
		{
			return string.Format("{0}?redirect_uri={1}&response_type=code&client_id={2}",
			                     config.OAuthAuthorizationEndpoint, redirectUri, config.OAuthIdentifier);
		}
	}
}