namespace EzBob.Backend.Strategies 
{
	using AmazonLib;
	using PayPal;
	using eBayLib;
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
	using PayPoint;
	using Sage;
	using StructureMap;
	using YodleeLib.connector;

	public class UpdateMarketplace : AStrategy
	{
		#region public

		#region constructor

		public UpdateMarketplace(int customerId, int marketplaceId, AConnection oDb, ASafeLog oLog) : base(oDb, oLog) {
			mailer = new StrategiesMailer(DB, Log);
			this.customerId = customerId;
			this.marketplaceId = marketplaceId;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Update Marketplaces"; }
		} // Name

		#endregion property Name

		#region method CustomerMarketPlaceAdded

		public override void Execute() {
			string errorMessage = string.Empty;
			DateTime startTime = DateTime.UtcNow;

			DataTable dt = DB.ExecuteReader(
				"GetMarketplaceDetailsForUpdate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("MarketplaceId", marketplaceId)
			);

			var sr = new SafeReader(dt.Rows[0]);
			string marketplaceName = sr["Name"];
			bool disabled = sr["Disabled"];

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
					new RetrieveDataHelper(Helper, new DatabaseMarketPlace(marketplaceName), Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(marketplaceName)).UpdateCustomerMarketplaceFirst(marketplaceId);
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
		
		#endregion public

		#region private

		#region property Helper

		private DatabaseDataHelper Helper {
			get { return ObjectFactory.GetInstance<DatabaseDataHelper>(); }
		} // Helper

		#endregion property Helper

		private readonly StrategiesMailer mailer;
		private readonly int customerId;
		private readonly int marketplaceId;

		#endregion private
	} // class UpdateMarketplace
} // namespace
