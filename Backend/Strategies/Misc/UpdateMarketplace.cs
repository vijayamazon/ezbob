namespace Ezbob.Backend.Strategies.Misc {
	using EZBob.DatabaseLib.Common;
	using EzBob.AmazonLib;
	using EzBob.PayPal;
	using EzBob.eBayLib;
	using MailStrategies.API;
	using DbConstants;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using EKM;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation;
	using Ezbob.Database;
	using FreeAgent;
	using Integration.ChannelGrabberFrontend;
	using PayPoint;
	using Sage;
	using YodleeLib.connector;

	public class UpdateMarketplace : AStrategy {
		public UpdateMarketplace(int customerId, int marketplaceId, bool doUpdateWizardStep) {
			mailer = new StrategiesMailer();
			this.customerId = customerId;
			this.marketplaceId = marketplaceId;
			m_bDoUpdateWizardStep = doUpdateWizardStep;
		} // constructor

		public override string Name {
			get { return "Update Marketplace"; }
		} // Name

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

			string marketplaceName = string.Empty;
			bool disabled = false;
			string marketplaceDisplayName = string.Empty;

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					marketplaceName = sr["Name"];
					disabled = sr["Disabled"];
					marketplaceDisplayName = sr["DisplayName"];
					return ActionResult.SkipAll;
				},
				"GetMarketplaceDetailsForUpdate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("MarketplaceId", marketplaceId)
			);

			if (disabled) {
				Log.Info("MP:{0} is disabled and won't be updated", marketplaceId);
				return;
			} // if

			int tokenExpired = 0;

			var oMpUpdateTimesSetter = new MarketplaceInstantUpdate(marketplaceId);

			Log.Info("Start Update Data for Customer Market Place: id: {0}, name: {1} ", marketplaceId, marketplaceDisplayName);

			try {
				oMpUpdateTimesSetter.Start();

				IMarketplaceRetrieveDataHelper oRetrieveDataHelper = null;
				var vi = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(marketplaceName);

				if (null != vi)
					oRetrieveDataHelper = new RetrieveDataHelper(DbHelper, new DatabaseMarketPlace(marketplaceName));
				else {
					switch (marketplaceName) {
					case "eBay":
						oRetrieveDataHelper = new eBayRetriveDataHelper(DbHelper, new eBayDatabaseMarketPlace());
						break;

					case "Amazon":
						oRetrieveDataHelper = new AmazonRetriveDataHelper(DbHelper, new AmazonDatabaseMarketPlace());
						break;

					case "Pay Pal":
						oRetrieveDataHelper = new PayPalRetriveDataHelper(DbHelper, new PayPalDatabaseMarketPlace());
						break;

					case "EKM":
						oRetrieveDataHelper = new EkmRetriveDataHelper(DbHelper, new EkmDatabaseMarketPlace());
						break;

					case "FreeAgent":
						oRetrieveDataHelper = new FreeAgentRetrieveDataHelper(DbHelper, new FreeAgentDatabaseMarketPlace());
						break;

					case "Sage":
						oRetrieveDataHelper = new SageRetrieveDataHelper(DbHelper, new SageDatabaseMarketPlace());
						break;

					case "PayPoint":
						oRetrieveDataHelper = new PayPointRetrieveDataHelper(DbHelper, new PayPointDatabaseMarketPlace());
						break;

					case "Yodlee":
						oRetrieveDataHelper = new YodleeRetriveDataHelper(DbHelper, new YodleeDatabaseMarketPlace());
						break;
					} // switch
				} // if

				if (oRetrieveDataHelper != null)
					oRetrieveDataHelper.Update(marketplaceId);
			} catch (Exception e) {
				errorMessage = e.Message;
				Log.Warn("Exception occurred during marketplace update, id: {0}.", marketplaceId);

				var variables = new Dictionary<string, string> {
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

				if (bHasEbayMsgNum) {
					tokenExpired = 1;

					variables.Add("MPType", marketplaceName);
					variables.Add("ErrorMessage", e.Message);
					variables.Add("ErrorCode", e.Message);

					sTemplateName = "Mandrill - Update MP Error Code";
				} else {
					variables.Add("UpdateCMP_Error", e.Message);
					sTemplateName = "Mandrill - UpdateCMP Error";
				} // if

				mailer.Send(sTemplateName, variables);
			} finally {
				Log.Info("End update data for umi: id: {0}, name: {1}. {2}", marketplaceId, marketplaceDisplayName, string.IsNullOrEmpty(errorMessage) ? "Successfully" : "With error!");

				oMpUpdateTimesSetter.End(errorMessage, tokenExpired);

				new SilentAutomation(this.customerId).SetTag(SilentAutomation.Callers.UpdateMarketplace).Execute();
			} // try
		} // Execute

		private readonly StrategiesMailer mailer;
		private readonly int customerId;
		private readonly int marketplaceId;
		private readonly bool m_bDoUpdateWizardStep;
	} // class UpdateMarketplace
} // namespace
