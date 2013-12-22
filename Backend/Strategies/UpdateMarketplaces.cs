using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using EKM;
using EZBob.DatabaseLib;
using Ezbob.Database;
using Ezbob.Logger;
using FreeAgent;
using Integration.ChannelGrabberFrontend;
using Integration.ChannelGrabberConfig;
using PayPoint;
using Sage;
using StructureMap;
using YodleeLib.connector;

namespace EzBob.Backend.Strategies {
	using AmazonLib;
	using PayPal;
	using eBayLib;

	public class UpdateMarketplaces : AStrategy {
		#region public

		#region constructor

		public UpdateMarketplaces(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			mailer = new StrategiesMailer(DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Update Marketplaces"; }
		} // Name

		#endregion property Name

		#region method CustomerMarketPlaceAdded

		public void CustomerMarketPlaceAdded(int customerId, int marketplaceId) {
			string errorMessage = string.Empty;
			DateTime startTime = DateTime.UtcNow;

			DataTable dt = DB.ExecuteReader(
				"GetMarketplaceDetailsForUpdate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("MarketplaceId", marketplaceId)
			);

			DataRow result = dt.Rows[0];
			string marketplaceName = result["Name"].ToString();
			bool disabled = bool.Parse(result["Disabled"].ToString());

			if (disabled)
				return;

			bool tokenExpired = false;

			try {
				if (null == Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(marketplaceName)) {
					switch (marketplaceName) {
					case "eBay":
						// TODO: make all the constructors empty, create helper and mp inside if needed
						new eBayRetriveDataHelper(Helper, new eBayDatabaseMarketPlace()).UpdateCustomerMarketplaceFirst(marketplaceId);
						break;

					case "Amazon":
						new AmazonRetriveDataHelper(Helper, new AmazonDatabaseMarketPlace()).UpdateCustomerMarketplaceFirst(marketplaceId);
						break;

					case "Pay Pal":
						new PayPalRetriveDataHelper(Helper, new PayPalDatabaseMarketPlace()).UpdateCustomerMarketplaceFirst(marketplaceId);
						break;

					case "EKM":
						new EkmRetriveDataHelper(Helper, new EkmDatabaseMarketPlace()).UpdateCustomerMarketplaceFirst(marketplaceId);
						break;

					case "FreeAgent":
						new FreeAgentRetrieveDataHelper(Helper, new FreeAgentDatabaseMarketPlace()).UpdateCustomerMarketplaceFirst(marketplaceId);
						break;

					case "Sage":
						new SageRetrieveDataHelper(Helper, new SageDatabaseMarketPlace()).UpdateCustomerMarketplaceFirst(marketplaceId);
						break;

					case "PayPoint":
						new PayPointRetrieveDataHelper(Helper, new PayPointDatabaseMarketPlace()).UpdateCustomerMarketplaceFirst(marketplaceId);
						break;

					case "Yodlee":
						new YodleeRetriveDataHelper(Helper, new YodleeDatabaseMarketPlace()).UpdateCustomerMarketplaceFirst(marketplaceId);
						break;
					} // switch
				}
				else
					new Integration.ChannelGrabberFrontend.RetrieveDataHelper(Helper, new DatabaseMarketPlace(marketplaceName), Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(marketplaceName)).UpdateCustomerMarketplaceFirst(marketplaceId);
			}
			catch (Exception e) {
				errorMessage = e.Message;
				string emailSubject, templateName;

				if (
					marketplaceName == "eBay" && (
						e.Message.Contains("16110") ||
						e.Message.Contains("931") ||
						e.Message.Contains("932") ||
						e.Message.Contains("16118") ||
						e.Message.Contains("16119") ||
						e.Message.Contains("17470")
					)
				) {
					tokenExpired = true;
					emailSubject = "eBay token has expired";
					templateName = "Mandrill - Update MP Error Code";

					var variables = new Dictionary<string, string> {
						{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
						{"MPType", marketplaceName},
						{"CustomerMarketPlaceId", marketplaceId.ToString(CultureInfo.InvariantCulture)},
						{"ErrorMessage", e.Message},
						{"ErrorCode", e.Message}
					};

					mailer.SendToEzbob(variables, templateName, emailSubject);
				}
				else {
					emailSubject = "ERROR occured when updating Customer Marketplace data";
					templateName = "Mandrill - UpdateCMP Error";

					var variables = new Dictionary<string, string> {
						{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
						{"CustomerMarketPlaceId", marketplaceId.ToString(CultureInfo.InvariantCulture)},
						{"UpdateCMP_Error", e.Message}
					};

					mailer.SendToEzbob(variables, templateName, emailSubject);
				} // if
			} // try

			DB.ExecuteNonQuery("UpdateMPErrorMP",
				CommandSpecies.StoredProcedure,
				new QueryParameter("umi", marketplaceId),
				new QueryParameter("UpdateError", errorMessage),
				new QueryParameter("TokenExpired", tokenExpired)
			);

			DB.ExecuteNonQuery("InsertStrategyMarketPlaceUpdateTime",
				CommandSpecies.StoredProcedure,
				new QueryParameter("MarketPlaceId", marketplaceId),
				new QueryParameter("StartDate", startTime),
				new QueryParameter("EndDate", DateTime.UtcNow)
			);
		} // CustomerMarketPlaceAdded

		#endregion method CustomerMarketPlaceAdded

		#region method UpdateAllMarketplaces

		public void UpdateAllMarketplaces(int customerId) {
			DataTable dt = DB.ExecuteReader(
				"GetCustomerMarketplaces",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);

			foreach (DataRow row in dt.Rows) {
				int marketplaceId = int.Parse(row["Id"].ToString());
				CustomerMarketPlaceAdded(customerId, marketplaceId);
			} // foreach
		} // UpdateAllMarketplaces

		#endregion method UpdateAllMarketplaces

		#endregion public

		#region private

		#region property Helper

		private DatabaseDataHelper Helper {
			get { return ObjectFactory.GetInstance<DatabaseDataHelper>(); }
		} // Helper

		#endregion property Helper

		private readonly StrategiesMailer mailer;

		#endregion private

		/* Original */
		//public void CustomerMarketPlaceAddedOriginal(int customerId, int marketplaceId)
		//{
		//	DateTime startTime = DateTime.UtcNow;

		//	var requestId = EzBob.RetrieveDataHelper.UpdateCustomerMarketplaceData(marketplaceId);

		//	// call InternalUpdateInfo directly

		//	while (!EzBob.RetrieveDataHelper.IsRequestDone(requestId))
		//	{
		//		Thread.Sleep(1000); // TODO: make this configurable
		//	}
		//	var requestState = EzBob.RetrieveDataHelper.GetRequestState(requestId);
		//	string errorCode = null;
		//	string errorMessage = null;
		//	bool tokenExpired = false;

		//	if (requestState == null || requestState.HasError())
		//	{
		//		DataTable dt = DbConnection.ExecuteSpReader("GetMarketplaceType");
		//		DataRow result = dt.Rows[0];
		//		string marketplaceType = result["Name"].ToString();

		//		if (requestState != null)
		//		{
		//			// TODO: make sure it contains the ebay error codes below such as token expired
		//			// What is the difference between errorCode & errorMessage?
		//			errorCode = requestState.ErorrInfo.Message;
		//			errorMessage = requestState.ErorrInfo.Message;
		//		}

		//		string emailSubject, templateName;

		//		if (marketplaceType == "eBay" &&
		//			(errorCode == "16110" || errorCode == "931" || errorCode == "932" || errorCode == "16118" ||
		//			 errorCode == "16119" || errorCode == "17470"))
		//		{
		//			tokenExpired = true;
		//			emailSubject = "eBay token has expired";
		//			templateName = "Mandrill - Update MP Error Code";

		//			var variables = new Dictionary<string, string>
		//				{
		//					{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
		//					{"MPType", marketplaceType},
		//					{"CustomerMarketPlaceId", marketplaceId.ToString(CultureInfo.InvariantCulture)},
		//					{"ErrorMessage", errorMessage},
		//					{"ErrorCode", errorCode}
		//				};

		//			mailer.SendToEzbob(variables, templateName, emailSubject);
		//		}
		//		else
		//		{
		//			emailSubject = "ERROR occured when updating Customer Marketplace data";
		//			templateName = "Mandrill - UpdateCMP Error";

		//			var variables = new Dictionary<string, string>
		//				{
		//					{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
		//					{"CustomerMarketPlaceId", marketplaceId.ToString(CultureInfo.InvariantCulture)},
		//					{"UpdateCMP_Error", errorMessage}
		//				};

		//			mailer.SendToEzbob(variables, templateName, emailSubject);
		//		}
		//	}

		//	DbConnection.ExecuteSpNonQuery("UpdateMPErrorMP",
		//		DbConnection.CreateParam("umi", marketplaceId),
		//		DbConnection.CreateParam("UpdateError", errorMessage),
		//		DbConnection.CreateParam("TokenExpired", tokenExpired));

		//	DbConnection.ExecuteSpNonQuery("InsertStrategyMarketPlaceUpdateTime",
		//		DbConnection.CreateParam("MarketPlaceId", marketplaceId),
		//		DbConnection.CreateParam("StartDate", startTime),
		//		DbConnection.CreateParam("EndDate", DateTime.UtcNow));
		//}

		//public void UpdateAllMarketplacesOriginal(int customerId)
		//{
		//	DataTable dt = DbConnection.ExecuteSpReader("GetCustomerMarketplaces", DbConnection.CreateParam("CustomerId", customerId));
		//	foreach (DataRow row in dt.Rows)
		//	{
		//		int marketplaceId = int.Parse(row["Id"].ToString());
		//		CustomerMarketPlaceAddedOriginal(customerId, marketplaceId);
		//	}
		//}
	} // class UpdateMarketplaces
} // namespace
