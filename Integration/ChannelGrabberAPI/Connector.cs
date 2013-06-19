using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using Integration.ChannelGrabberConfig;
using Newtonsoft.Json;
using DBCustomer = EZBob.DatabaseLib.Model.Database.Customer;
using RestSharp;
using log4net;
using Scorto.Configuration;

namespace Integration.ChannelGrabberAPI {
	#region enum LogMsgTypes

	public enum LogMsgTypes {
		Info,
		Error,
		Warn,
		Fatal,
		Debug
	} // LogMsgTypes

	#endregion enum LogMsgTypes

	#region class Connector

	public class Connector : ConfigurationRoot {
		#region public

		#region constructor

		public Connector(AccountData oAccountData, ILog log, DBCustomer oCustomer) {
			m_oAccountData = oAccountData;

			m_oLog = log;

			Debug("Creating a ChannelGrabber API Connector class...");

			if (oCustomer == null)
				throw new ApiException("Customer information not specified.");

			m_oCustomer = oCustomer;

			ConfigurationRoot o = EnvironmentConfiguration.Configuration.GetCurrentConfiguration<ConfigurationRoot>();

			string sServiceUrl = o.GetValueWithDefault<string>("ChannelGrabberServiceUrl", string.Empty);

			if (sServiceUrl == string.Empty)
				throw new ApiException("Service URL not specified.");

			Debug("Validating ChannelGrabber Service URL {0}", sServiceUrl);

			m_oRestClient = new RestClient(sServiceUrl);

			XmlNodeList lst = ExecuteRequest().SelectNodes(ServiceValidateXpath);

			if ((null == lst) || (2 > lst.Count))
				throw new ApiException("Failed to parse Service output.");

			Debug("Creating a ChannelGrabber API Connector class succeeded.");
		} // constructor

		#endregion constructor

		#region property ShopTypeName

		public virtual string ShopTypeName {
			get { return m_oAccountData.AccountTypeName(); }
		} // ShopTypeName

		#endregion property ShopTypeName

		#region method Validate

		public virtual void Validate() {
			Info("Validate {0} customer started with parameter [ {1} ]", ShopTypeName, m_oAccountData);

			Dictionary<string, Customer> oCustomers = LoadCustomers();

			Customer oCustomer = null;

			if (oCustomers.ContainsKey(m_oCustomer.Name))
				oCustomer = oCustomers[m_oCustomer.Name];
			else {
				Info("Customer not found.");

				var ri = new RegionInfo(DefaultRegion);
				oCustomer = CreateCustomer(m_oCustomer.Name, ri.ThreeLetterISORegionName);
			} // if

			if ((oCustomer == null) || !oCustomer.IsValid())
				throw new ApiException("Failed to load customer details.");

			Debug("Customer details loaded successfully.");

			ValidateShop(oCustomer, m_oAccountData);

			Info("Validate {0} customer complete.", ShopTypeName);
		} // Validate

		#endregion method Validate

		#region method GetOrders

		public List<Order> GetOrders() {
			Info(
				"GetOrders for {0} customer {1} ({2}) started with parameters [ {3} ]",
				ShopTypeName, m_oCustomer.Name, m_oCustomer.Id, m_oAccountData
			);

			Dictionary<string, Customer> oCustomers = LoadCustomers();

			Customer oCustomer =
				oCustomers.ContainsKey(m_oCustomer.Name)
					? oCustomers[m_oCustomer.Name]
					: null;

			if ((oCustomer == null) || !oCustomer.IsValid())
				throw new ApiException("Failed to load customer details.");

			Debug("Verifying account id...");

			m_oAccountData.VerifyAccountID(
				ExecuteRequest(BuildRegisterShopRq(oCustomer))
			);

			int nRqID = SendGenerateOrdersRq(oCustomer, m_oAccountData);

			OrderFetchStatus nRes = FetchOrdersRq(oCustomer, m_oAccountData, nRqID);

			while (nRes == OrderFetchStatus.NotReady) {
				Debug("Not ready, sleeping...");
				Thread.Sleep(SleepTime);

				nRes = FetchOrdersRq(oCustomer, m_oAccountData, nRqID);
			} // forever

			if (nRes == OrderFetchStatus.Complete) {
				List<Order> lst = LoadOrders(oCustomer, m_oAccountData);

				Info("GetOrders for {0} customer {1} ({2}) complete, {3} order{4} received.",
					ShopTypeName, m_oCustomer.Name, m_oCustomer.Id,
					lst.Count, lst.Count == 1 ? "" : "s"
				);

				return lst;
			} // if

			Info("GetOrders for {0} customer {1} ({2}) completed with error, no orders received.",
				ShopTypeName, m_oCustomer.Name, m_oCustomer.Id
			);

			return new List<Order>();
		} // GetOrders

		#endregion method GetOrders

		#endregion public

		#region protected

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

		private Dictionary<string, Customer> LoadCustomers() {
			var oCustomers = new Dictionary<string, Customer>();

			Info("Loading list of customers started...");

			XmlNodeList oNodeList = ExecuteRequest(CustomersRq).SelectNodes(CustomerXpath);

			if (oNodeList == null) {
				Info("No customers found");
				return oCustomers;
			} // if

			foreach (XmlNode oNode in oNodeList) {
				var oCustomer = new Customer(oNode);

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

		private void ValidateShop(Customer oCustomer, AccountData oAccountData) {
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

		private Customer CreateCustomer(string sCustomerName, string sCountry) {
			Debug("Creating Channel Grabber customer {0} from {1}...", sCustomerName, sCountry);

			var oCustomer = new Customer(sCustomerName, sCountry);

			oCustomer.FromXml(ExecuteRequest(CustomersRq, oCustomer));

			if (!oCustomer.IsValid()) {
				Error("Failed to create customer: invalid data. {0}", oCustomer);
				throw new ApiException("Failed to create customer: invalid data returned.");
			} // if

			Debug("Customer details: {0}", oCustomer);
			Debug("Creating Channel Grabber customer {0} from {1} complete.", sCustomerName, sCountry);

			return oCustomer;
		} // CreateCustomer

		#endregion method CreateCustomer

		#endregion Validate related only

		#region GetOrders related only

		#region SendGenerateOrdersRq

		private int SendGenerateOrdersRq(Customer oCustomer, AccountData oAccountData) {
			Debug("Sending generate orders request...");
			XmlDocument doc = ExecutePostRequest(BuildGenerateOrdersRq(oCustomer, oAccountData));
			return XmlUtil.GetInt(doc, XmlUtil.IdNode);
		} // SendGenerateOrdersRq

		#endregion SendGenerateOrdersRq

		#region FetchOrdersRq

		private OrderFetchStatus FetchOrdersRq(Customer oCustomer, AccountData oAccountData, int nRqID) {
			Debug("Tesing whether orders are ready...");

			XmlDocument doc = ExecuteRequest(BuildOrdersGeneratedRq(oCustomer, oAccountData, nRqID));

			if (XmlUtil.IsError(doc)) {
				Error("Error while fetching orders: {0}", XmlUtil.GetError(doc));
				return OrderFetchStatus.Error;
			} // if

			return XmlUtil.IsComplete(doc) ? OrderFetchStatus.Complete : OrderFetchStatus.NotReady;
		} // FetchOrdersRq

		#endregion FetchOrdersRq

		#region LoadOrders

		private List<Order> LoadOrders(Customer oCustomer, AccountData oAccountData) {
			Debug("Loading list of orders...");

			var lst = new List<Order>();

			XmlDocument doc = ExecuteRequest(BuildOrdersRq(oCustomer));

			string sShopTypeName = ShopTypeName.ToLower();

			foreach (XmlNode oNode in doc.DocumentElement.ChildNodes) {
				var o = Order.Create(oNode, sShopTypeName, oAccountData);

				if (o != null)
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
					throw new ConnectionFailException(sErrorMsg);

				throw new ApiException(sErrorMsg);
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
				throw new ApiException(sErrorMsg);
			} // try

			if (null == doc.DocumentElement) {
				var sErrorMsg = string.Format("Request fail: failed to parse response: no root node.");
				Error(sErrorMsg);
				throw new ApiException(sErrorMsg);
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
			object oJsonData = null;

			var oHeaders = new Dictionary<string, string>();
			oHeaders["Accept"] = "application/xml";

			var oRequest = new RestRequest(sResource, nMethod);

			if (null != oData) {
				oHeaders["Content-Type"] = "application/json";
				oHeaders["x-li-format"] = "json";

				oRequest.RequestFormat = DataFormat.Json;

				oJsonData = oData.ToJson();
				oRequest.AddBody(oJsonData);
			} // if

			var aryHeaders = new List<string>();

			foreach (KeyValuePair<string, string> h in oHeaders) {
				oRequest.AddHeader(h.Key, h.Value);
				aryHeaders.Add(string.Format("{0}: {1}", h.Key, h.Value));
			} // for each

			Debug(@"
*******************************************
*
* Request details - begin
*
*******************************************

Method: {0}

Resourse: {1}

Headers: {2}

Data: {3}

*******************************************
*
* Request details - end
*
*******************************************
", nMethod.ToString(), sResource, string.Join("\n         ", aryHeaders), oJsonData == null ? "-- no data --" : JsonConvert.SerializeObject(oJsonData));

			return oRequest;
		} // CreateRequest

		#endregion method CreateRequest

		#region method BuildRegisterShopRq

		private string BuildRegisterShopRq(Customer oCustomer) {
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

		private string BuildValidityRq(Customer oCustomer, AccountData oAccountData) {
			return string.Format(
				ValidityReportRq, oCustomer.Id, ShopTypeName.ToLower(), oAccountData.Id()
			);
		} // BuildValidityRq

		#endregion method BuildRegisterShopRq

		#region method BuildGenerateOrdersRq

		private string BuildGenerateOrdersRq(Customer oCustomer, AccountData oAccountData) {
			return string.Format(
				GenerateOrdersRq, oCustomer.Id, ShopTypeName.ToLower(), oAccountData.Id()
			);
		} // BuildGenerateOrdersRq

		#endregion method BuildGenerateOrdersRq

		#region method BuildOrdersGeneratedRq

		private string BuildOrdersGeneratedRq(Customer oCustomer, AccountData oAccountData, int nRqID) {
			return string.Format(
				OrdersGeneratedRq, oCustomer.Id, ShopTypeName.ToLower(), oAccountData.Id(), nRqID
			);
		} // BuildOrdersGeneratedRq

		#endregion method BuildOrdersGeneratedRq

		#region method BuildOrdersRq

		private string BuildOrdersRq(Customer oCustomer) {
			return string.Format(OrdersRq, oCustomer.Id);
		} // BuildOrdersRq

		#endregion method BuildOrdersRq

		#region private fields

		private ILog m_oLog;
		private DBCustomer m_oCustomer;
		private RestClient m_oRestClient;
		private AccountData m_oAccountData;

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
	} // class Connector

	#endregion class Connector
} // namespace ChannelGrabberAPI
