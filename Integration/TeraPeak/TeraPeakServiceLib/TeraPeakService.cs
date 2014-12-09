using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using EzBob.CommonLib;
using EzBob.CommonLib.MarketplaceSpecificTypes.TeraPeakOrdersData;
using EzBob.TeraPeakServiceLib.Requests;
using EzBob.TeraPeakServiceLib.Requests.SellerResearch;
using log4net;

namespace EzBob.TeraPeakServiceLib
{
	using Ezbob.Utils.Serialization;

	public interface ITeraPeakService
	{
		GetSellerResearchResults SearchBySeller(string sellerId, ResultSellerInfo resultSellerInfo, SearchQueryDatesRange searchQueryDates);
	}

	public class TeraPeakService : ITeraPeakService
	{
		private WebRequest CreateWebRequest()
		{
			//_StatusCode = default( HttpStatusCode );

			//var webRequest = WebRequest.Create( new Uri( "http://api.terapeak.com" ) );
			//var webRequest = WebRequest.Create( new Uri( "http://api.datavision.com" ) );
			//var webRequest = WebRequest.Create( new Uri( "http://api.dataunison.com" ) );

			var webRequest = WebRequest.Create( CreateUrl() );
			webRequest.ContentType = "application/x-www-form-urlencoded";
			//req.ContentLength = 500;
			webRequest.Method = "POST";

			return webRequest;
		}

		private Uri CreateUrl()
		{
			string result;

			string apiKey = ConfigManager.CurrentValues.Instance.TeraPeakApiKey;
			string url = ConfigManager.CurrentValues.Instance.TeraPeakUrl;
			if (!string.IsNullOrEmpty( apiKey ))
			{
				result = string.Format( "{0}?api_key={1}", url, apiKey );
			}
			else
			{
				result = url;
			}

			return new Uri( result );
		}

		public static TeraPeakDatabaseSellerData SearchBySeller(TeraPeakRequestInfo requestInfo)
		{
			var service = new TeraPeakService();
			var req = new TeraPeakSearchBySellerRequester( service );

			return req.Run( requestInfo );
		}

		/*public string SearchByProduct(string searchString)
		{

			SearchQuery search = new SearchQuery
								{
									Keywords = searchString,
									//SiteId = "3",//"eBay.co.uk",
									//Currency = "1" //"USD"
								};
			var req = new GetResearchResultsRequest( _CredentionProvider.RequesterCredentials, search );

			var requestString = CreateRequestString( req );

			return DoRequest( requestString );

		}*/

		private string CreateRequestString<T>( T req ) where T : ServiceRequestDataBase {
			return new Serialized( req );
		}

		private string DoRequest( string requestString )			
		{
			WebRequest webRequest = CreateWebRequest();
			SendRequest( webRequest, requestString );
			return GetResponce(webRequest);
		}

		private string CreateRequestStringSearchBySeller(string sellerId, ResultSellerInfo resultSellerInfo, SearchQueryDates searchQueryDates)
		{
			var req = new GetSellerResearchResultsRequest(null, sellerId, resultSellerInfo, searchQueryDates);
			return CreateRequestString( req );
		}

		private void SendRequest(WebRequest webRequest, string queryString )			
		{
			byte[] requestData = Encoding.UTF8.GetBytes( queryString );
			using ( var stream = webRequest.GetRequestStream() )
			{
				stream.Write( requestData, 0, requestData.Length );
			}

		}

		private string GetResponce(WebRequest webRequest)
		{
			string responseBody = null;

			using ( var httpResponse = webRequest.GetResponse() as HttpWebResponse )
			{
				//_StatusCode = httpResponse.StatusCode;
				using ( var reader = new StreamReader( httpResponse.GetResponseStream(), Encoding.UTF8 ) )
				{
					responseBody = reader.ReadToEnd();
				}
			}

			return responseBody;
		}

		public GetSellerResearchResults SearchBySeller(string sellerId, ResultSellerInfo resultSellerInfo, SearchQueryDatesRange searchQueryDates)
		{
			string requestString = CreateRequestStringSearchBySeller( sellerId, resultSellerInfo, searchQueryDates );
			WriteToLog( string.Format( "TeraPeak Request :\n{0}", requestString ) );
			string resultString = DoRequest( requestString );
			WriteToLog( string.Format( "TeraPeak Response :\n{0}", resultString ) );
			return ParseResult( resultString );
		}

		private GetSellerResearchResults ParseResult( string resultString )
		{
			return Serialized.Deserialize<GetSellerResearchResults>( resultString );
		}

		protected void WriteToLog( string message, WriteLogType messageType = WriteLogType.Info, Exception ex = null )
		{
			WriteLoggerHelper.Write(message, messageType, null, ex);
		}
	}

	public enum TeraPeakRequestStepEnum
	{
		ByMonth,
	}

	public class TeraPeakRequestDataInfo
	{
		public DateTime StartDate { get; set; }
		public TeraPeakRequestStepEnum StepType { get; set; }
		public int CountSteps { get; set; }
	}

}

