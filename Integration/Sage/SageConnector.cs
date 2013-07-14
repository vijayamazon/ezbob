namespace Sage
{
    using System;
    using System.Collections.Generic;
    using System.Web.Script.Serialization;
    using Config;
    using EZBob.DatabaseLib.DatabaseWrapper.Order;
    using RestSharp;
	using log4net;
	using StructureMap;

	public class SageConnector
	{
		private static readonly ISageConfig config = ObjectFactory.GetInstance<ISageConfig>();
		private static readonly ILog log = LogManager.GetLogger(typeof(SageConnector));

		public static SageInvoicesList GetInvoices(string accessToken, DateTime? fromDate)
		{
			string monthPart = !fromDate.HasValue ? string.Empty : string.Format("?{0}", string.Format(config.InvoicesRequestMonthPart, fromDate.Value.ToString("dd/MM/yyyy"), DateTime.UtcNow.ToString("dd/MM/yyyy")).Replace('-', '/'));
			string timedInvoicesRequest = string.Format("{0}{1}", config.InvoicesRequest, monthPart);
			var request = new RestRequest(Method.GET) { Resource = timedInvoicesRequest };
			request.AddHeader("Authorization", "Bearer " + accessToken);

			var client = new RestClient();

			IRestResponse response = client.Execute(request);
			var invoices = new List<SageInvoice>();
			SageInvoicesListHelper deserializeInvoicesResponse = FillInvoicesFromResponse(response, invoices);
			if (deserializeInvoicesResponse == null)
			{
				log.Error("Sage invoices were not fetched");
				return new SageInvoicesList(DateTime.UtcNow, invoices);
			}

			string nextUrl = GetNextUrl(deserializeInvoicesResponse, timedInvoicesRequest);

			while (nextUrl != null)
			{
				request = new RestRequest(Method.GET) { Resource = nextUrl };
				request.AddHeader("Authorization", "Bearer " + accessToken);
				response = client.Execute(request);

				deserializeInvoicesResponse = FillInvoicesFromResponse(response, invoices);
				if (deserializeInvoicesResponse == null)
				{
					log.Error("Sage invoices were not fetched");
					return new SageInvoicesList(DateTime.UtcNow, new List<SageInvoice>());
				}

				nextUrl = GetNextUrl(deserializeInvoicesResponse, timedInvoicesRequest);
			}

			var sageInvoicesList = new SageInvoicesList(DateTime.UtcNow, invoices);
			return sageInvoicesList;
        }

		private static SageInvoicesListHelper FillInvoicesFromResponse(IRestResponse response, List<SageInvoice> invoices)
		{
			var js = new JavaScriptSerializer();
			string cleanResponse = response.Content.Replace("\"$", "\"");
			var deserializeInvoicesResponse = DeserializeInvoices(cleanResponse, js);
			if (deserializeInvoicesResponse == null)
			{
				log.Error("Error deserializing sage invoices");
				return null;
			}
			if (deserializeInvoicesResponse.diagnoses != null)
			{
				foreach (SageDiagnostic diagnostic in deserializeInvoicesResponse.diagnoses)
				{
					log.ErrorFormat("Error occured during sage invoices request. Invoices were not fetched. Message:{0} Source:{1} Severity:{2} DataCode:{3}", 
						diagnostic.message, diagnostic.source, diagnostic.severity, diagnostic.dataCode);
				}
				return null;
			}

			foreach (var serializaedInvoice in deserializeInvoicesResponse.resources)
			{
				TryDeserializeInvoice(invoices, serializaedInvoice);
			}

			return deserializeInvoicesResponse;
		}

		private static void TryDeserializeInvoice(List<SageInvoice> invoices, SageInvoiceSerialization serializaedInvoice)
		{
			try
			{
				invoices.Add(SageDesreializer.DeserializeInvoice(serializaedInvoice));
			}
			catch (Exception)
			{
				log.ErrorFormat("Failed creating invoice for SageId:{0}. Invoice won't be handled!", serializaedInvoice.SageId);
			}
		}

		private static SageInvoicesListHelper DeserializeInvoices(string invoicesResponse, JavaScriptSerializer js)
		{
			try
			{
				return ((SageInvoicesListHelper)js.Deserialize(invoicesResponse, typeof(SageInvoicesListHelper)));
			}
			catch (Exception e)
			{
				log.ErrorFormat("Failed deserializing sage invoices response:{0}. The error was:{1}", invoicesResponse, e);
				return null;
			}
		}

		private static string GetNextUrl(SageInvoicesListHelper serializedResponse, string timedInvoicesRequest)
		{
			int nextIndex = serializedResponse.startIndex + serializedResponse.itemsPerPage;
			if (serializedResponse.totalResults <= nextIndex)
			{
				return null;
			}

			return string.Format("{0}{1}$startIndex={2}", timedInvoicesRequest, timedInvoicesRequest.Contains("?") ? "&" : "?", nextIndex);
		}
		
		public static AccessTokenContainer GetToken(string code, string redirectVal, out string errorMessage)
		{
			string accessTokenRequest = string.Format("{0}?grant_type=authorization_code&code={1}&redirect_uri={2}&scope=&client_secret={3}&client_id={4}",
													  config.OAuthTokenEndpoint, code, redirectVal, config.OAuthSecret, config.OAuthIdentifier);
			
			var request = new RestRequest(Method.POST) { Resource = accessTokenRequest };
			request.AddHeader("Accept", "application / json");
			var client = new RestClient();
			errorMessage = null;
			try
			{
				IRestResponse response = client.Execute(request);
				if (response != null)
				{
					var js = new JavaScriptSerializer();
					var accessTokenContainer = (AccessTokenContainer)js.Deserialize(response.Content, typeof(AccessTokenContainer));
					return accessTokenContainer;
				}
			}
			catch (Exception e)
			{
				errorMessage = "Failure getting access token";
				log.WarnFormat("{0}. Exception:{1}", errorMessage, e);
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