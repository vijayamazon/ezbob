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
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Database;
	using FreeAgent;
	using global::Integration.ChannelGrabberFrontend;
	using PayPoint;
	using Sage;
	using YodleeLib.connector;

	public class UpdateMarketplace : AStrategy {
		public UpdateMarketplace(int customerId, int marketplaceId, bool doUpdateWizardStep) {
			this.marketplaceUpdateStatus = null;
			this.doSilentAutomation = true;

			this.mailer = new StrategiesMailer();
			this.customerId = customerId;
			this.marketplaceId = marketplaceId;
			this.doUpdateWizardStep = doUpdateWizardStep;
		} // constructor

		public override string Name {
			get { return "Update Marketplace"; }
		} // Name

		public UpdateMarketplace PreventSilentAutomation() {
			this.doSilentAutomation = false;
			return this;
		} // PreventSilentAutomation

		public override void Execute() {
			string errorMessage = string.Empty;

			if (this.doUpdateWizardStep) {
				DB.ExecuteNonQuery(
					"CustomerSetWizardStepIfNotLast",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@CustomerID", this.customerId),
					new QueryParameter("@NewStepID", (int)WizardStepType.Marketplace)
				);
			} // if

			string marketplaceName = string.Empty;
			bool disabled = false;
			string marketplaceDisplayName = string.Empty;
			bool firstTime = false;
			bool longUpdateTime = false;

			SafeReader sr = DB.GetFirst(
				"GetMarketplaceDetailsForUpdate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("MarketplaceId", this.marketplaceId)
			);

			if (!sr.IsEmpty) {
				marketplaceName = sr["Name"];
				disabled = sr["Disabled"];
				marketplaceDisplayName = sr["DisplayName"];
				firstTime = sr["FirstTime"];
				longUpdateTime = sr["LongUpdateTime"];
			} // if

			if (disabled) {
				Log.Info("MP:{0} is disabled and won't be updated", this.marketplaceId);
				return;
			} // if

			if (longUpdateTime) {
				Context.Description = string.Format(
					"This strategy can take long time (updating {0} marketplace with id {1}).",
					marketplaceName,
					this.marketplaceId
				);
			} // if

			int tokenExpired = 0;

			var oMpUpdateTimesSetter = new MarketplaceInstantUpdate(this.marketplaceId);

			Log.Info(
				"Start Update Data for Customer Market Place: id: {0}, name: {1} ",
				this.marketplaceId,
				marketplaceDisplayName
			);

			try {
				oMpUpdateTimesSetter.Start();

				IMarketplaceRetrieveDataHelper oRetrieveDataHelper = null;
				var vi = global::Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(marketplaceName);

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
					oRetrieveDataHelper.Update(this.marketplaceId);
			} catch (Exception e) {
				errorMessage = e.Message;
				Log.Warn("Exception occurred during marketplace update, id: {0}.", this.marketplaceId);

				var variables = new Dictionary<string, string> {
					{"userID", this.customerId.ToString(CultureInfo.InvariantCulture)},
					{"CustomerMarketPlaceId", this.marketplaceId.ToString(CultureInfo.InvariantCulture)},
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

				this.mailer.Send(sTemplateName, variables);
			} finally {
				Log.Info(
					"End update data for umi: id: {0}, name: {1}. {2}",
					this.marketplaceId,
					marketplaceDisplayName,
					string.IsNullOrEmpty(errorMessage) ? "Successfully" : "With error!"
				);

				oMpUpdateTimesSetter.End(errorMessage, tokenExpired);

				if (this.marketplaceUpdateStatus != null)
					this.marketplaceUpdateStatus.NotifyDone(this.marketplaceId);

				if (this.doSilentAutomation) {
					new SilentAutomation(this.customerId).SetTag(firstTime
						? SilentAutomation.Callers.AddMarketplace
						: SilentAutomation.Callers.UpdateMarketplace
					) .Execute();
				} // if
			} // try
		} // Execute

		internal UpdateMarketplace SetMarketplaceUpdateStatus(MarketplaceUpdateStatus mpus) {
			this.marketplaceUpdateStatus = mpus;
			return this;
		} // SetMarketplaceUpdateStatus

		private readonly StrategiesMailer mailer;
		private readonly int customerId;
		private readonly int marketplaceId;
		private readonly bool doUpdateWizardStep;
		private bool doSilentAutomation;
		private MarketplaceUpdateStatus marketplaceUpdateStatus;
	} // class UpdateMarketplace
} // namespace
