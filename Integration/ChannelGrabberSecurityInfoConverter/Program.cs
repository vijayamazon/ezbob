using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using EzBob.CommonLib;
using Ezbob.Database;
using Ezbob.Logger;
using Integration.ChannelGrabberConfig;
using SqlConnection = Ezbob.Database.SqlConnection;

namespace Integration.ChannelGrabberSecurityInfoConverter {
	class Program {

		static void Main(string[] args) {
			string sContext = string.Empty;

			if ((args.Length > 1) && (args[0] == "--context"))
				sContext = args[1];

			var app = new Program();

			if (app.Init(sContext))
				app.Run();

			app.Done();
		} // Main

		private bool Init(string sContext) {
			m_oLog = new ConsoleLog(new LegacyLog());

			m_oVendors = new Dictionary<Guid, VendorInfo>();

			Configuration.Instance.ForEachVendor(vi => {
				var guid = new Guid(vi.InternalID);

				m_oVendors[guid] = vi;

				Console.WriteLine("{0}: {1}", vi.Name, guid);
			});

			m_oDB = new SqlConnection(m_oLog, GetConnectionString(sContext));

			return true;
		} // Init

		private string GetConnectionString(string sContext) {
			if (sContext == string.Empty)
				return null;

			try {
				string sConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[sContext].ConnectionString;

				m_oLog.Info(string.Format("ConnectionString: {0}", sConnectionString));

				return sConnectionString;
			}
			catch (Exception e) {
				string sMsg = string.Format(
					"Failed to load connection string from configuration file using name {0}",
					sContext
				);

				m_oLog.Fatal(sMsg + ": " + e.Message);

				throw new Exception(sMsg, e);
			} // try
		} // GetConnectionString

		private void Run() {
			DataTable tbl = m_oDB.ExecuteReader(RetrieveSecurityDataQuery);

			var bck = new FileSaver(m_oLog);

			foreach (DataRow row in tbl.Rows) {
				var nID      = (int)row[0];
				var sName    = row[1].ToString();
				var oOldData = (byte[])row[2];
				var sType    = row[3].ToString();
				var guid     = new Guid(row[4].ToString());

				if (!m_oVendors.ContainsKey(guid))
					continue;

				m_oLog.Info("Marketplace {0} ({1}) of type {2}", sName, nID, sType);

				AccountModel am = ConvertSecurityInfo(oOldData, sType);

				if (am == null)
					continue;

				string sSerialisedData = SerializeDataHelper.SerializeToString(am);

				m_oLog.Info("New security info: {0}", sSerialisedData);

				if (!bck.Save(nID, oOldData))
					continue;

				bck.Save(nID, am);

				m_oDB.ExecuteNonQuery(
					"UPDATE MP_CustomerMarketPlace SET SecurityData = CONVERT(VARBINARY(4096), @info) WHERE Id = @id",
					CommandSpecies.Text,
					new QueryParameter("@info", sSerialisedData),
					new QueryParameter("@id", nID)
				);
			} // for each row

			tbl.Dispose();
		} // Run

		private AccountModel ConvertSecurityInfo(byte[] oOldData, string sAccountTypeName) {
			string sXml = System.Text.Encoding.Default.GetString(oOldData);

			m_oLog.Debug("Old security info: {0}", sXml);

			XmlDocument doc = new XmlDocument();

			try {
				doc.LoadXml(sXml);
			}
			catch (Exception e) {
				m_oLog.Error("Failed to convert old security data to XML: {0}\n\nData:\n\n{1}", e.Message, sXml);
				return null;
			} // try

			if (doc.DocumentElement == null) {
				m_oLog.Error("No root element detected in conversion from: {0}", sXml);
				return null;
			} // if

			switch (doc.DocumentElement.Name) {
			case "VolusionSecurityInfo":
				return ToModel(doc, sAccountTypeName, AccountModel.NodeNames.MarketplaceId, AccountModel.NodeNames.Url, AccountModel.NodeNames.Login, AccountModel.NodeNames.Password, AccountModel.NodeNames.DisplayName);

			case "PlaySecurityInfo":
				return ToModel(doc, sAccountTypeName, AccountModel.NodeNames.MarketplaceId, AccountModel.NodeNames.Name, AccountModel.NodeNames.Login, AccountModel.NodeNames.Password);

			case "SecurityInfo":
				return SecurityInfoToModel(doc, sAccountTypeName);

			default:
				m_oLog.Error("Unsupported data format: {0}", doc.DocumentElement.Name);
				return null;
			} // switch
		} // ConvertSecurityInfo

		private AccountModel ToModel(XmlDocument doc, string sAccountTypeName, params AccountModel.NodeNames[] aryNodeNames) {
			var am = new AccountModel();

			foreach (AccountModel.NodeNames nNodeName in aryNodeNames) {
				XmlNode oNode = doc.DocumentElement.SelectSingleNode(nNodeName.ToString());

				if (oNode == null) {
					m_oLog.Error("{0} is not found in old security data", nNodeName.ToString());
					return null;
				} // if

				am.Set(nNodeName, oNode.InnerText);
			} // for each

			am.accountTypeName = sAccountTypeName;

			am.Validate();

			return am;
		} // ToModel

		private AccountModel SecurityInfoToModel(XmlDocument doc, string sAccountTypeName) {
			var am = new AccountModel();

			var oNodeNames = new Dictionary<string, AccountModel.NodeNames> {
				{AccountModel.NodeNames.MarketplaceId.ToString(), AccountModel.NodeNames.MarketplaceId},
				{"AccountData/Name", AccountModel.NodeNames.Name},
				{"AccountData/URL", AccountModel.NodeNames.Url},
				{"AccountData/Login", AccountModel.NodeNames.Login},
				{"AccountData/Password", AccountModel.NodeNames.Password},
				{"AccountData/LimitDays", AccountModel.NodeNames.LimitDays},
				{"AccountData/AuxLogin", AccountModel.NodeNames.AuxLogin},
				{"AccountData/AuxPassword", AccountModel.NodeNames.AuxPassword},
				{"AccountData/RealmID", AccountModel.NodeNames.RealmId},
			};

			foreach (KeyValuePair<string, AccountModel.NodeNames> nn in oNodeNames) {
				XmlNode oNode = doc.DocumentElement.SelectSingleNode(nn.Key);

				if (oNode == null)
					continue;

				am.Set(nn.Value, oNode.InnerText);
			} // for each

			am.accountTypeName = sAccountTypeName;

			am.Validate();

			return am;
		} // SecurityInfoToModel

		private string RetrieveSecurityDataQuery {
			get { return @"SELECT
	m.Id,
	m.DisplayName,
	m.SecurityData,
	t.Name,
	t.InternalId
FROM
	MP_CustomerMarketPlace m
	INNER JOIN MP_MarketplaceType t ON m.MarketPlaceId = t.Id";
			} // get
		} // RetrieveSecurityDataQuery

		private void Done() {
		} // Done

		private IConnection m_oDB;
		private ASafeLog m_oLog;
		private Dictionary<Guid, VendorInfo> m_oVendors;
	} // class Program
} // namespace
