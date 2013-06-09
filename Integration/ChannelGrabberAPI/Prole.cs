using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Xml;
using EZBob.DatabaseLib.Model.Database;
using RestSharp;
using log4net;
using Scorto.Configuration;

namespace Integration.ChannelGrabberAPI {
	#region enum ShopTypes

	public enum ShopTypes {
		Volusion,
		Kashflow,
		Magento,
		Play,
	} // enum ShopTypes

	#endregion enum ShopTypes

	#region enum LogMsgTypes

	public enum LogMsgTypes {
		Info,
		Error,
		Warn,
		Fatal,
		Debug
	} // LogMsgTypes

	#endregion enum LogMsgTypes

	#region class Prole

	public abstract class Prole : ConfigurationRoot {
		#region public

		#region property ShopType

		public abstract ShopTypes ShopType { get; }

		#endregion property ShopType

		#region property ShopTypeName

		public string ShopTypeName { get { return ShopType.ToString(); } }

		#endregion property ShopTypeName

		#region method Validate

		public virtual void Validate(IAccountData oAccountData) {
			Info("Validate {0} customer started with parameter [ {1} ]", ShopTypeName, oAccountData);

			Dictionary<string, ChannelGrabberCustomer> oCustomers = LoadCustomers();

			ChannelGrabberCustomer oCustomer = null;

			if (oCustomers.ContainsKey(m_oCustomer.Name))
				oCustomer = oCustomers[m_oCustomer.Name];
			else {
				Info("Customer not found.");

				var ri = new RegionInfo(DefaultRegion);
				oCustomer = CreateCustomer(m_oCustomer.Name, ri.ThreeLetterISORegionName);
			} // if

			if ((oCustomer == null) || !oCustomer.IsValid())
				throw new ChannelGrabberApiException("Failed to load customer details.");

			Debug("Customer details loaded successfully.");

			ValidateShop(oCustomer, oAccountData);

			Info("Validate {0} customer complete.", ShopTypeName);
		} // Validate

		#endregion method Validate

		#region method GetOrders

		public List<ChannelGrabberOrder> GetOrders(IAccountData oAccountData) {
			Info(
				"GetOrders for {0} customer {1} ({2}) started with parameters [ {3} ]",
				ShopTypeName, m_oCustomer.Name, m_oCustomer.Id, oAccountData
			);

			Dictionary<string, ChannelGrabberCustomer> oCustomers = LoadCustomers();

			ChannelGrabberCustomer oCustomer =
				oCustomers.ContainsKey(m_oCustomer.Name)
					? oCustomers[m_oCustomer.Name]
					: null;

			if ((oCustomer == null) || !oCustomer.IsValid())
				throw new ChannelGrabberApiException("Failed to load customer details.");

			Debug("Verifying account id...");

			oAccountData.VerifyAccountID(
				ExecuteRequest(BuildRegisterShopRq(oCustomer))
			);

			int nRqID = SendGenerateOrdersRq(oCustomer, oAccountData);

			OrderFetchStatus nRes = FetchOrdersRq(oCustomer, oAccountData, nRqID);

			while (nRes == OrderFetchStatus.NotReady) {
				Debug("Not ready, sleeping...");
				Thread.Sleep(SleepTime);

				nRes = FetchOrdersRq(oCustomer, oAccountData, nRqID);
			} // forever

			if (nRes == OrderFetchStatus.Complete) {
				List<ChannelGrabberOrder> lst = LoadOrders(oCustomer, oAccountData);

				Info("GetOrders for {0} customer {1} ({2}) complete, {3} order{4} received.",
					ShopTypeName, m_oCustomer.Name, m_oCustomer.Id,
					lst.Count, lst.Count == 1 ? "" : "s"
				);

				return lst;
			} // if

			Info("GetOrders for {0} customer {1} ({2}) completed with error, no orders received.",
				ShopTypeName, m_oCustomer.Name, m_oCustomer.Id
			);

			return new List<ChannelGrabberOrder>();
		} // GetOrders

		#endregion method GetOrders

		#endregion public

		#region protected

		#region constructor

		protected Prole(ILog log, Customer oCustomer) {
			m_oLog = log;

			Debug("Creating a ChannelGrabber API prole class...");

			if (oCustomer == null)
				throw new ChannelGrabberApiException("Customer information not specified.");

			m_oCustomer = oCustomer;

			ConfigurationRoot o = EnvironmentConfiguration.Configuration.GetCurrentConfiguration<ConfigurationRoot>();

			string sServiceUrl = o.GetValueWithDefault<string>("ChannelGrabberServiceUrl", string.Empty);

			if (sServiceUrl == string.Empty)
				throw new ChannelGrabberApiException("ChannelGrabber Service URL not specified.");

			Debug("Validating ChannelGrabber Service URL {0}", sServiceUrl);

			m_oRestClient = new RestClient(sServiceUrl);

			XmlNodeList lst = ExecuteRequest().SelectNodes(ServiceValidateXpath);

			if ((null == lst) || (2 > lst.Count))
				throw new ChannelGrabberApiException("Failed to parse ChannelGrabber Service output.");

			Debug("Creating a ChannelGrabber API prole class succeeded.");
		} // constructor

		#endregion constructor

		#region method Say

		protected void Say(LogMsgTypes nMsgType, string sFormat, params object[] args) {
			Say(nMsgType, string.Format(sFormat, args));
		} // Say

		protected void Say(LogMsgTypes nMsgType, string sMsg) {
			if ((m_oLog == null) || (sMsg == null))
				return;

			const string bol = "\n\n";
			const string eol = "\n";

			switch (nMsgType) {
				case LogMsgTypes.Error:
					if (m_oLog.IsErrorEnabled)
						m_oLog.Error(bol + sMsg + eol);
					break;

				case LogMsgTypes.Warn:
					if (m_oLog.IsWarnEnabled)
						m_oLog.Warn(bol + sMsg + eol);
					break;

				case LogMsgTypes.Info:
					if (m_oLog.IsInfoEnabled)
						m_oLog.Info(bol + sMsg + eol);
					break;

				case LogMsgTypes.Debug:
					if (m_oLog.IsDebugEnabled)
						m_oLog.Debug(bol + sMsg + eol);
					break;

				case LogMsgTypes.Fatal:
					if (m_oLog.IsFatalEnabled)
						m_oLog.Fatal(bol + sMsg + eol);
					break;
			} // switch
		} // Say

		#endregion method Say

		#region method Info

		protected void Info(string sMsg) { Say(LogMsgTypes.Info, sMsg); } // Info

		protected void Info(string sFormat, params object[] args) { Info(string.Format(sFormat, args)); } // Info

		#endregion Info

		#region method Error

		protected void Error(string sMsg) { Say(LogMsgTypes.Error, sMsg); } // Error

		protected void Error(string sFormat, params object[] args) { Error(string.Format(sFormat, args)); } // Error

		#endregion Error

		#region method Warn

		protected void Warn(string sMsg) { Say(LogMsgTypes.Warn, sMsg); } // Warn

		protected void Warn(string sFormat, params object[] args) { Warn(string.Format(sFormat, args)); } // Warn

		#endregion Warn

		#region method Fatal

		protected void Fatal(string sMsg) { Say(LogMsgTypes.Fatal, sMsg); } // Msg

		protected void Fatal(string sFormat, params object[] args) { Fatal(string.Format(sFormat, args)); } // Fatal

		#endregion Fatal

		#region method Debug

		protected void Debug(string sMsg) { Say(LogMsgTypes.Debug, sMsg); } // Debug

		protected void Debug(string sFormat, params object[] args) { Debug(string.Format(sFormat, args)); } // Debug

		#endregion Debug

		#endregion protected

		#region private

		#region method LoadCustomers

		private Dictionary<string, ChannelGrabberCustomer> LoadCustomers() {
			var oCustomers = new Dictionary<string, ChannelGrabberCustomer>();

			Info("Loading list of customers started...");

			XmlNodeList oNodeList = ExecuteRequest(CustomersRq).SelectNodes(CustomerXpath);

			if (oNodeList == null) {
				Info("No customers found");
				return oCustomers;
			} // if

			foreach (XmlNode oNode in oNodeList) {
				var oCustomer = new ChannelGrabberCustomer(oNode);

				if (oCustomer.IsValid()) {
					oCustomers[oCustomer.Name] = oCustomer;
					Debug("Customer loaded: {0}", oCustomer);
				} // if
			} // foreach

			Info("Loading list of customers complete, {0} customer{1} loaded.", oCustomers.Count, oCustomers.Count == 1 ? "" : "s");
			return oCustomers;
		} // LoadCustomers

		#endregion method LoadCustomers

		#region Validate related only

		#region method ValidateShop

		private void ValidateShop(ChannelGrabberCustomer oCustomer, IAccountData oAccountData) {
			Debug("Validating shop details...");

			string sRequest = BuildRegisterShopRq(oCustomer);

			oAccountData.VerifyNotExist(ExecuteRequest(sRequest));

			oAccountData.VerifyRegistrationInProgress(
				ExecuteRequest(sRequest, oAccountData)
			);

			Debug("Shop registration request sent.");

			oAccountData.Validate(ExecutePostRequest(BuildValidityRq(oCustomer, oAccountData)));

			Debug("Validating shop details complete.");
		} // ValidateShop

		#endregion method ValidateShop

		#region method CreateCustomer

		private ChannelGrabberCustomer CreateCustomer(string sCustomerName, string sCountry) {
			Debug("Creating Channel Grabber customer {0} from {1}...", sCustomerName, sCountry);

			var oCustomer = new ChannelGrabberCustomer(sCustomerName, sCountry);

			oCustomer.FromXml(ExecuteRequest(CustomersRq, oCustomer));

			if (!oCustomer.IsValid()) {
				Error("Failed to create customer: invalid data. {0}", oCustomer);
				throw new ChannelGrabberApiException("Failed to create customer: invalid data returned.");
			} // if

			Debug("Customer details: {0}", oCustomer);
			Debug("Creating Channel Grabber customer {0} from {1} complete.", sCustomerName, sCountry);

			return oCustomer;
		} // CreateCustomer

		#endregion method CreateCustomer

		#endregion Validate related only

		#region GetOrders related only

		#region SendGenerateOrdersRq

		private int SendGenerateOrdersRq(ChannelGrabberCustomer oCustomer, IAccountData oAccountData) {
			Debug("Sending generate orders request...");
			XmlDocument doc = ExecutePostRequest(BuildGenerateOrdersRq(oCustomer, oAccountData));
			return API.GetInt(doc, API.IdNode);
		} // SendGenerateOrdersRq

		#endregion SendGenerateOrdersRq

		#region FetchOrdersRq

		private OrderFetchStatus FetchOrdersRq(ChannelGrabberCustomer oCustomer, IAccountData oAccountData, int nRqID) {
			Debug("Tesing whether orders are ready...");

			XmlDocument doc = ExecuteRequest(BuildOrdersGeneratedRq(oCustomer, oAccountData, nRqID));

			if (API.IsError(doc)) {
				Error("Error while fetching orders: {0}", API.GetError(doc));
				return OrderFetchStatus.Error;
			} // if

			return API.IsComplete(doc) ? OrderFetchStatus.Complete : OrderFetchStatus.NotReady;
		} // FetchOrdersRq

		#endregion FetchOrdersRq

		#region LoadOrders

		private List<ChannelGrabberOrder> LoadOrders(ChannelGrabberCustomer oCustomer, IAccountData oAccountData) {
			Debug("Loading list of orders...");

			var lst = new List<ChannelGrabberOrder>();

			XmlDocument doc = ExecuteRequest(BuildOrdersRq(oCustomer));

			foreach (XmlNode oNode in doc.DocumentElement.ChildNodes) {
				var o = new ChannelGrabberOrder(oNode);
				lst.Add(o);
			} // foreach

			Debug("Loading list of orders complete, {0} order{1} loaded.", lst.Count, lst.Count == 1 ? "" : "s");
			return lst;
		} // LoadOrders

		#endregion LoadOrders

		#endregion GetOrders related only

		#region infrastructure

		#region method ExecuteRequest

		private XmlDocument ExecutePostRequest(string sResource) {
			return ExecuteRequest(CreatePostRequest(sResource));
		} // ExecutePostRequest

		private XmlDocument ExecuteRequest(string sResource = "", IJsonable oData = null) {
			sResource = (sResource ?? string.Empty).Trim();

			if (sResource == string.Empty)
				return ExecuteRequest(CreateRequest(sResource));

			return ExecuteRequest(CreateRequest(sResource, oData));
		} // ExecuteRequest

		private XmlDocument ExecuteRequest(RestRequest oRequest) {
			Debug("Requesting {0} {1}/{2}", oRequest.Method, m_oRestClient.BaseUrl, oRequest.Resource);

			IRestResponse oResponse = m_oRestClient.Execute(oRequest);

			Debug("Response status: {0}\nResponse output: {1}", oResponse.StatusCode, oResponse.Content);

			if (oResponse.StatusCode != HttpStatusCode.OK) {
				var sErrorMsg = string.Format(
					"Request fail: failed to execute request.\nStatus: {0} - {1}\nError msg: {2}",
					oResponse.StatusCode,
					oResponse.StatusCode.ToString(),
					oResponse.ErrorMessage
				);

				Error(sErrorMsg);

				if (oResponse.StatusCode == 0)
					throw new ConnectionFailChannelGrabberApiException(sErrorMsg);
				else
					throw new ChannelGrabberApiException(sErrorMsg);
			} // if

			var doc = new XmlDocument();

			try {
				doc.LoadXml(oResponse.Content);
			}
			catch (Exception e) {
				var sErrorMsg = string.Format(
					"Request fail: failed to parse response: {0}", e.Message
				);

				Error(sErrorMsg);
				throw new ChannelGrabberApiException(sErrorMsg);
			} // try

			if (null == doc.DocumentElement) {
				var sErrorMsg = string.Format("Request fail: failed to parse response: no root node.");
				Error(sErrorMsg);
				throw new ChannelGrabberApiException(sErrorMsg);
			} // if

			Debug("Response output parsed successfully.\nRequest succeeded.");

			return doc;
		} // ExecuteRequest

		#endregion method ExecuteRequest

		#region method CreateRequest

		private RestRequest CreatePostRequest(string sResource) {
			var oRequest = new RestRequest(sResource, Method.POST);
			oRequest.AddHeader("Accept", "application/xml");
			return oRequest;
		} // CreatePostRequest

		private RestRequest CreateRequest(string sResource = "", IJsonable oData = null) {
			Method nMethod = oData == null ? Method.GET : Method.POST;

			var oRequest = new RestRequest(sResource, nMethod);
			oRequest.AddHeader("Accept", "application/xml");

			if (null != oData) {
				oRequest.AddHeader("Content-Type", "application/json");
				oRequest.AddHeader("x-li-format", "json");

				oRequest.RequestFormat = DataFormat.Json;
				oRequest.AddBody(oData.ToJson());
			} // if

			return oRequest;
		} // CreateRequest

		#endregion method CreateRequest

		#region method BuildRegisterShopRq

		private string BuildRegisterShopRq(ChannelGrabberCustomer oCustomer) {
			return string.Format(
				RegisterShopRq, oCustomer.Id, ShopTypeName.ToLower()
			);
		} // BuildRegisterShopRq

		#endregion method BuildRegisterShopRq

		#region enum OrderFetchStatus

		private enum OrderFetchStatus {
			Complete,
			Error,
			NotReady
		} // enum OrderFetchStatus

		#endregion enum OrderFetchStatus

		#region method BuildValidityRq

		private string BuildValidityRq(ChannelGrabberCustomer oCustomer, IAccountData oAccountData) {
			return string.Format(
				ValidityReportRq, oCustomer.Id, ShopTypeName.ToLower(), oAccountData.Id()
			);
		} // BuildValidityRq

		#endregion method BuildRegisterShopRq

		#region method BuildGenerateOrdersRq

		private string BuildGenerateOrdersRq(ChannelGrabberCustomer oCustomer, IAccountData oAccountData) {
			return string.Format(
				GenerateOrdersRq, oCustomer.Id, ShopTypeName.ToLower(), oAccountData.Id()
			);
		} // BuildGenerateOrdersRq

		#endregion method BuildGenerateOrdersRq

		#region method BuildOrdersGeneratedRq

		private string BuildOrdersGeneratedRq(ChannelGrabberCustomer oCustomer, IAccountData oAccountData, int nRqID) {
			return string.Format(
				OrdersGeneratedRq, oCustomer.Id, ShopTypeName.ToLower(), oAccountData.Id(), nRqID
			);
		} // BuildOrdersGeneratedRq

		#endregion method BuildOrdersGeneratedRq

		#region method BuildOrdersRq

		private string BuildOrdersRq(ChannelGrabberCustomer oCustomer) {
			return string.Format(OrdersRq, oCustomer.Id);
		} // BuildOrdersRq

		#endregion method BuildOrdersRq

		#region private fields

		private ILog m_oLog;
		private Customer m_oCustomer;
		private RestClient m_oRestClient;

		#endregion private fields

		#region private const

		private const string DefaultRegion = "en-GB";

		private const string CustomersRq = "customers";
		private const string RegisterShopRq = CustomersRq + "/{0}/{1}";
		private const string ValidityReportRq = CustomersRq + "/{0}/{1}/{2}/validityReport";
		private const string GenerateOrdersRq = CustomersRq + "/{0}/{1}/{2}/orderReport";
		private const string OrdersGeneratedRq = CustomersRq + "/{0}/{1}/{2}/orderReport/{3}";
		private const string OrdersRq = CustomersRq + "/{0}/orders";

		private const string CustomerXpath = "/resource/resource";
		private const string ServiceValidateXpath = "/resource/link[@rel]";

		private const int SleepTime = 1000;

		#endregion private const

		#endregion infrastructure

		#endregion private
	} // class Prole

	#endregion class Prole
} // namespace ChannelGrabberAPI
