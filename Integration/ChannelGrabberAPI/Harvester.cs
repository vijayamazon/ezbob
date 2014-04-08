﻿namespace Integration.ChannelGrabberAPI {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Net;
	using System.Threading;
	using System.Xml;
	using Ezbob.Logger;
	using ChannelGrabberConfig;
	using Newtonsoft.Json;
	using DBCustomer = EZBob.DatabaseLib.Model.Database.Customer;
	using RestSharp;
	using log4net;
	using EZBob.DatabaseLib.Model;
	using StructureMap;

	#region class Harvester

	public class Harvester : SafeILog, IHarvester {
		#region public

		#region constructor

		public Harvester(AccountData oAccountData, ILog log, DBCustomer oCustomer) : base(log) {
			m_oAccountData = oAccountData;

			if (oCustomer == null)
				throw new ApiException("Customer information not specified.");

			m_oCustomer = oCustomer;

			configurationVariablesRepository = ObjectFactory.GetInstance<ConfigurationVariablesRepository>();
		} // constructor

		#endregion constructor

		#region method Init

		public virtual bool Init() {
			RetrievedOrders = null;

			Debug("Creating a ChannelGrabber API Harvester class...");

			string sServiceUrl = configurationVariablesRepository.GetByName("ChannelGrabberServiceUrl");
			m_nSleepTime = 1000 * configurationVariablesRepository.GetByNameAsInt("ChannelGrabberSleepTime");
			m_nWaitCycleCount = (ulong)configurationVariablesRepository.GetByNameAsInt("ChannelGrabberCycleCount");

			Debug("Validating ChannelGrabber Service URL {0}", sServiceUrl);

			m_oRestClient = new RestClient(sServiceUrl);

			XmlNodeList lst = ExecuteRequest().SelectNodes(ServiceValidateXpath);

			if ((null == lst) || (2 > lst.Count))
				throw new ApiException("Failed to parse Service output.");

			Debug("When waiting for orders:\n\tsleep for {0:N} ms between poll attempts\n\tretry polling {1:N} times", m_nSleepTime, m_nWaitCycleCount);

			Debug("Creating a ChannelGrabber API Harvester class succeeded.");

			return true;
		} // Init

		#endregion method Init

		#region method Run

		public virtual void Run(bool bValidateCredentialsOnly, int nCustomerMarketplaceID) {
			Run(bValidateCredentialsOnly);
		} // Run

		public virtual void Run(bool bValidateCredentialsOnly) {
			if (bValidateCredentialsOnly)
				Validate();
			else
				RetrievedOrders = GetOrders();
		} // Run

		#endregion method Run

		#region method Done

		public virtual void Done() {
			// nothing here
		} // Done

		#endregion method Done

		#region property RetrievedOrders

		public virtual List<Order> RetrievedOrders { get; private set; }

		#endregion property RetrievedOrders

		#endregion public

		#region private

		#region method Validate

		private void Validate() {
			Info("Validate {0} customer started with parameter [ {1} ]", m_oAccountData.AccountTypeName(), m_oAccountData);

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

			Info("Validate {0} customer complete.", m_oAccountData.AccountTypeName());
		} // Validate

		#endregion method Validate

		#region method GetOrders

		private List<Order> GetOrders() {
			Info(
				"GetOrders for {0} customer {1} ({2}) started with parameters [ {3} ]",
				m_oAccountData.AccountTypeName(), m_oCustomer.Name, m_oCustomer.Id, m_oAccountData
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

			int nOrdRqID = SendGenerateOrdExpRq(GenerateOrdersRq, oCustomer, m_oAccountData);
			int nExpRqID = m_oAccountData.VendorInfo.HasExpenses ? SendGenerateOrdExpRq(GenerateExpensesRq, oCustomer, m_oAccountData) : 0;

			var oOrdRes = new OrdExpFetchResult {
				Status = OrderFetchStatus.NotReady,
				Data = null
			};

			var oExpRes = new OrdExpFetchResult {
				Status = m_oAccountData.VendorInfo.HasExpenses ? OrderFetchStatus.NotReady : OrderFetchStatus.Complete,
				Data = null
			};

			var oOrdExpList = new List<Order>();

			for (ulong wcc = 0; wcc < m_nWaitCycleCount; wcc++) {
				if (oOrdRes.Status == OrderFetchStatus.NotReady) {
					oOrdRes = FetchOrdExpRq(OrdersGeneratedRq, oCustomer, m_oAccountData, nOrdRqID);

					if (oOrdRes.Status == OrderFetchStatus.Complete)
						LoadOrdExp(oOrdExpList, oOrdRes.Data, 0, m_oAccountData);
				} // if

				if (oExpRes.Status == OrderFetchStatus.NotReady) {
					oExpRes = FetchOrdExpRq(ExpensesGeneratedRq, oCustomer, m_oAccountData, nExpRqID);

					if (oExpRes.Status == OrderFetchStatus.Complete)
						LoadOrdExp(oOrdExpList, oExpRes.Data, 1, m_oAccountData);
				} // if

				if ((oOrdRes.Status != OrderFetchStatus.NotReady) && (oExpRes.Status != OrderFetchStatus.NotReady))
					break;

				Debug("Fetch orders status: {0}, fetch expenses status: {1}, sleeping...", oOrdRes.Status, oExpRes.Status);
				Thread.Sleep(m_nSleepTime);
			} // for

			Info("GetOrders for {0} customer {1} ({2}) complete, {3} {4} received.",
				m_oAccountData.AccountTypeName(), m_oCustomer.Name, m_oCustomer.Id,
				oOrdExpList.Count, oOrdExpList.Count == 1 ? "entry" : "entries"
			);

			return oOrdExpList;
		} // GetOrders

		#endregion method GetOrders

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

		private int SendGenerateOrdExpRq(string sRequest, Customer oCustomer, AccountData oAccountData) {
			Debug("Sending generate orders request...");
			XmlDocument doc = ExecutePostRequest(BuildGenerateOrdExpRq(sRequest, oCustomer, oAccountData));
			return XmlUtil.GetInt(doc, XmlUtil.IdNode);
		} // SendGenerateOrdExpRq

		#endregion SendGenerateOrdExpRq

		#region struct OrdExpFetchResult

		private struct OrdExpFetchResult {
			public OrderFetchStatus Status;
			public XmlDocument Data;
		} // OrdExpFetchResult

		#endregion struct OrdExpFetchResult

		#region FetchOrdExpRq

		private OrdExpFetchResult FetchOrdExpRq(string sRequest, Customer oCustomer, AccountData oAccountData, int nRqID) {
			Debug("Tesing whether request is complete...");

			var oRes = new OrdExpFetchResult {
				Status = OrderFetchStatus.NotReady,
				Data = ExecuteRequest(BuildOrdExpGeneratedRq(sRequest, oCustomer, oAccountData, nRqID))
			};

			if (XmlUtil.IsError(oRes.Data)) {
				oRes.Status = OrderFetchStatus.Error;

				Error("Error while fetching request status: {0}", XmlUtil.GetError(oRes.Data));
			} // if
			else
				oRes.Status = XmlUtil.IsComplete(oRes.Data) ? OrderFetchStatus.Complete : OrderFetchStatus.NotReady;

			return oRes;
		} // FetchOrdExpRq

		#endregion FetchOrdExpRq

		#region LoadOrdExp

		private void LoadOrdExp(List<Order> lst, XmlDocument oData, int nIsExpense, AccountData oAccountData) {
			Debug("Loading list of {0}s...", nIsExpense == 0 ? "order": "expense");

			string sShopTypeName = m_oAccountData.AccountTypeName().ToLower();

			uint nCount = 0;

			foreach (XmlNode oNode in oData.DocumentElement.ChildNodes) {
				if (oNode.Name != "resource")
					continue;

				var o = Order.Create(oNode, sShopTypeName, oAccountData, nIsExpense);

				if (o != null) {
					lst.Add(o);
					nCount++;
				} // if
			} // foreach

			Debug("Loading list of {2}s complete, {0} {2}{1} loaded.", nCount, nCount == 1 ? "" : "s", nIsExpense == 0 ? "order" : "expense");
		} // LoadOrdExp

		#endregion LoadOrdExp

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
				RegisterShopRq, oCustomer.Id, m_oAccountData.AccountTypeName().ToLower()
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
				ValidityReportRq, oCustomer.Id, m_oAccountData.AccountTypeName().ToLower(), oAccountData.Id()
			);
		} // BuildValidityRq

		#endregion method BuildRegisterShopRq

		#region method BuildGenerateOrdExpRq

		private string BuildGenerateOrdExpRq(string sRequest, Customer oCustomer, AccountData oAccountData) {
			return string.Format(
				sRequest, oCustomer.Id, m_oAccountData.AccountTypeName().ToLower(), oAccountData.Id()
			);
		} // BuildGenerateOrdExpRq

		#endregion method BuildGenerateOrdExpRq

		#region method BuildOrdExpGeneratedRq

		private string BuildOrdExpGeneratedRq(string sRequest, Customer oCustomer, AccountData oAccountData, int nRqID) {
			return string.Format(
				sRequest, oCustomer.Id, m_oAccountData.AccountTypeName().ToLower(), oAccountData.Id(), nRqID
			);
		} // BuildOrdExpGeneratedRq

		#endregion method BuildOrdExpGeneratedRq
		
		#region private fields

		private readonly DBCustomer m_oCustomer;
		private readonly AccountData m_oAccountData;
		private ConfigurationVariablesRepository configurationVariablesRepository;

		private RestClient m_oRestClient;

		private int m_nSleepTime;
		private ulong m_nWaitCycleCount;

		#endregion private fields

		#region private const

		private const string DefaultRegion = "en-GB";

		private const string CustomersRq = "customers";
		private const string RegisterShopRq = CustomersRq + "/{0}/{1}";
		private const string ValidityReportRq = CustomersRq + "/{0}/{1}/{2}/validityReport";
		private const string GenerateOrdersRq = CustomersRq + "/{0}/{1}/{2}/orderReport";
		private const string GenerateExpensesRq = CustomersRq + "/{0}/{1}/{2}/expenseReport";
		private const string OrdersGeneratedRq = CustomersRq + "/{0}/{1}/{2}/orderReport/{3}";
		private const string ExpensesGeneratedRq = CustomersRq + "/{0}/{1}/{2}/expenseReport/{3}";

		private const string CustomerXpath = "/resource/resource";
		private const string ServiceValidateXpath = "/resource/link[@rel]";

		private const string ServiceUrlCfg = "ChannelGrabberServiceUrl";
		private const string SleepTimeCfg = "ChannelGrabberSleepTime";
		private const string CycleCountCfg = "ChannelGrabberCycleCount";

		#endregion private const

		#endregion infrastructure

		#endregion private
	} // class Harvester

	#endregion class Harvester
} // namespace ChannelGrabberAPI
