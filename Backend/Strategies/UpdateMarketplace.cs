namespace EzBob.Backend.Strategies {
	using AmazonLib;
	using MailStrategies.API;
	using PayPal;
	using eBayLib;
	using DbConstants;
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

		public UpdateMarketplace(int customerId, int marketplaceId, bool doUpdateWizardStep, AConnection oDb, ASafeLog oLog) : base(oDb, oLog) {
			mailer = new StrategiesMailer(DB, Log);
			this.customerId = customerId;
			this.marketplaceId = marketplaceId;
			m_bDoUpdateWizardStep = doUpdateWizardStep;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Update Marketplace"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			string errorMessage = string.Empty;

			if (m_bDoUpdateWizardStep) {
				DB.ExecuteNonQuery(
					"CustomerSetWizardStepIfNotLast",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@CustomerID", customerId),
					new QueryParameter("@NewStepID", (int)WizardStepType.Marketplace)
				);
			} // if

			DataTable dt = DB.ExecuteReader(
				"GetMarketplaceDetailsForUpdate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("MarketplaceId", marketplaceId)
			);

			var sr = new SafeReader(dt.Rows[0]);
			string marketplaceName = sr["Name"];
			bool disabled = sr["Disabled"];
			string marketplaceDisplayName = sr["DisplayName"];

			if (disabled)
			{
				Log.Info("MP:{0} is disabled and won't be updated", marketplaceId);
				return;
			}

			int tokenExpired = 0;
			int historyItemId = 0;

			Log.Info("Activating retrieve data helper of:{0}", marketplaceName);
			try
			{
				historyItemId = UpdateCustomerMarketPlaceDataStart(marketplaceDisplayName);
				if (null == Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(marketplaceName))
				{
					switch (marketplaceName)
					{
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
							new FreeAgentRetrieveDataHelper(Helper, new FreeAgentDatabaseMarketPlace()).UpdateCustomerMarketplaceFirst(
								marketplaceId);
							break;

						case "Sage":
							new SageRetrieveDataHelper(Helper, new SageDatabaseMarketPlace()).UpdateCustomerMarketplaceFirst(marketplaceId);
							break;

						case "PayPoint":
							new PayPointRetrieveDataHelper(Helper, new PayPointDatabaseMarketPlace()).UpdateCustomerMarketplaceFirst(
								marketplaceId);
							break;

						case "Yodlee":
							new YodleeRetriveDataHelper(Helper, new YodleeDatabaseMarketPlace()).UpdateCustomerMarketplaceFirst(marketplaceId);
							break;
					} // switch
				}
				else
					new RetrieveDataHelper(Helper, new DatabaseMarketPlace(marketplaceName),
					                       Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(marketplaceName))
						.UpdateCustomerMarketplaceFirst(marketplaceId);
			}
			catch (Exception e)
			{
				errorMessage = e.Message;
				Log.Warn("Exception occured during mp update. id:{0}", marketplaceId);

				var variables = new Dictionary<string, string>
					{
						{"userID", customerId.ToString(CultureInfo.InvariantCulture)},
						{"CustomerMarketPlaceId", marketplaceId.ToString(CultureInfo.InvariantCulture)},
					};

				bool bHasEbayMsgNum = marketplaceName == "eBay" && (
					                                                   e.Message.Contains("16110") ||
					                                                   e.Message.Contains("931") ||
					                                                   e.Message.Contains("932") ||
					                                                   e.Message.Contains("16118") ||
					                                                   e.Message.Contains("16119") ||
					                                                   e.Message.Contains("17470")
				                                                   );

				string sTemplateName;

				if (bHasEbayMsgNum)
				{
					tokenExpired = 1;

					variables.Add("MPType", marketplaceName);
					variables.Add("ErrorMessage", e.Message);
					variables.Add("ErrorCode", e.Message);

					sTemplateName = "Mandrill - Update MP Error Code";
				}
				else
				{
					variables.Add("UpdateCMP_Error", e.Message);

					sTemplateName = "Mandrill - UpdateCMP Error";
				} // if

				mailer.Send(sTemplateName, variables);
			} // try
			finally
			{
				UpdateCustomerMarketPlaceDataEnd(historyItemId, errorMessage, marketplaceDisplayName, tokenExpired);
			}
		} // Execute

		#endregion method Execute
		
		#endregion public

		#region private

		private int UpdateCustomerMarketPlaceDataStart(string marketplaceDisplayName)
		{
			Log.Info("Start Update Data for Customer Market Place: id: {0}, name: {1} ", marketplaceId, marketplaceDisplayName);
			DateTime updatingStart = DateTime.UtcNow;
			DataTable dt = DB.ExecuteReader("StartMarketplaceUpdate", CommandSpecies.StoredProcedure, 
				new QueryParameter("MarketplaceId", marketplaceId), 
				new QueryParameter("UpdatingStart", updatingStart));

			var sr = new SafeReader(dt.Rows[0]);
			return sr["HistoryRecordId"];
		}

		private void UpdateCustomerMarketPlaceDataEnd(int historyItemId, string errorMessage, string marketplaceDisplayName, int tokenExpired)
		{
			Log.Info("End update data for umi: id: {0}, name: {1}. {2}", marketplaceId, marketplaceDisplayName, string.IsNullOrEmpty(errorMessage) ? "Successfully" : "With error!");
			DateTime updatingEnd = DateTime.UtcNow;
			DB.ExecuteNonQuery("EndMarketplaceUpdate", CommandSpecies.StoredProcedure,
				new QueryParameter("MarketplaceId", marketplaceId),
				new QueryParameter("HistoryRecordId", historyItemId),
				new QueryParameter("UpdatingEnd", updatingEnd),
				new QueryParameter("ErrorMessage", errorMessage),
				new QueryParameter("TokenExpired", tokenExpired));
		}

		#region property Helper

		private DatabaseDataHelper Helper {
			get { return ObjectFactory.GetInstance<DatabaseDataHelper>(); }
		} // Helper

		#endregion property Helper

		private readonly StrategiesMailer mailer;
		private readonly int customerId;
		private readonly int marketplaceId;
		private readonly bool m_bDoUpdateWizardStep;

		#endregion private
	} // class UpdateMarketplace
} // namespace
