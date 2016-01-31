namespace Ezbob.Backend.Strategies.MainStrategy.Steps {
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Backend.Strategies.MainStrategy.Helpers;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Utils.Lingvo;
	using EZBob.DatabaseLib.Model.Database;

	internal class UpdateData : AOneExitStep {
		public UpdateData(
			string outerContextDescription,
			int customerID,
			int marketplaceUpdateValidityDays,
			StrategiesMailer mailer,
			bool logicalGlueEnabled,
			int monthlyPayment
		) : base(outerContextDescription) {
			this.customerID = customerID;
			this.marketplaceUpdateValidityDays = marketplaceUpdateValidityDays;
			this.mailer = mailer;
			this.logicalGlueEnabled = logicalGlueEnabled;
			this.monthlyPayment = monthlyPayment;
		} // constructor

		protected override void ExecuteStep() {
			MarketplaceUpdateStatus mpus = UpdateMarketplaces();

			new Staller(this.customerID, this.mailer)
				.SetMarketplaceUpdateStatus(mpus)
				.Stall();

			ExecuteAdditionalStrategies();
		} // ExecuteStep

		private MarketplaceUpdateStatus UpdateMarketplaces() {
			Log.Debug("Checking which marketplaces should be updated for {0}...", OuterContextDescription);

			DateTime now = DateTime.UtcNow;

			var mpsToUpdate = new List<int>();

			DB.ForEachRowSafe(
				sr => {
					DateTime? lastUpdateStart = sr["UpdatingStart"];
					DateTime? lastUpdateEnd = sr["UpdatingEnd"];

					bool shouldUpdate = (lastUpdateStart == null) || ((
						(lastUpdateEnd != null) &&
						(lastUpdateEnd.Value.AddDays(this.marketplaceUpdateValidityDays) <= now)
					));

					if (!shouldUpdate)
						return;

					mpsToUpdate.Add(sr["MpID"]);
				},
				"LoadMarketplacesLastUpdateTime",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@CustomerID", this.customerID)
			);

			if (mpsToUpdate.Count < 1) {
				Log.Debug("No marketplace should be updated for {0}.", OuterContextDescription);
				return null;
			} // if

			Log.Debug(
				"{0} to update for {1}: {2}.",
				Grammar.Number(mpsToUpdate.Count, "Marketplace"),
				OuterContextDescription,
				string.Join(", ", mpsToUpdate)
			);

			var mpus = new MarketplaceUpdateStatus(mpsToUpdate);

			foreach (int mpID in mpsToUpdate) {
				int thisMpID = mpID; // to avoid "Access to foreach variable in closure".

				Task.Run(() => {
					Log.Debug("Updating marketplace {0} for {1}...", thisMpID, OuterContextDescription);

					new UpdateMarketplace(this.customerID, thisMpID, false)
						.PreventSilentAutomation()
						.SetMarketplaceUpdateStatus(mpus)
						.Execute();

					Log.Debug("Updating marketplace {0} for {1} complete.", thisMpID, OuterContextDescription);
				});
			} // for each

			Log.Debug(
				"Update launched for marketplaces {1} for {0}.",
				OuterContextDescription,
				string.Join(", ", mpsToUpdate)
			);

			return mpus;
		} // UpdateMarketplaces

		private void DoConsumerCheck(PreliminaryData preData) {
			new ExperianConsumerCheck(this.customerID, null, false).PreventSilentAutomation().Execute();

			if (preData.TypeOfBusiness != TypeOfBusiness.Entrepreneur) {
				Strategies.Library.Instance.DB.ForEachRowSafe(
					sr => {
						int appDirId = sr["DirId"];
						string appDirName = sr["DirName"];
						string appDirSurname = sr["DirSurname"];

						if (string.IsNullOrEmpty(appDirName) || string.IsNullOrEmpty(appDirSurname))
							return;

						var directorExperianConsumerCheck = new ExperianConsumerCheck(this.customerID, appDirId, false);
						directorExperianConsumerCheck.Execute();
					},
					"GetCustomerDirectorsForConsumerCheck",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", this.customerID)
				);
			} // if
		} // DoConsumerCheck

		private void DoCompanyCheck(PreliminaryData preData) {
			if (preData.LastStartedMainStrategyEndTime.HasValue)
				new ExperianCompanyCheck(this.customerID, false).PreventSilentAutomation().Execute();
		} // DoCompanyCheck

		private void DoAmlCheck(PreliminaryData preData) {
			if (preData.LastStartedMainStrategyEndTime.HasValue)
				new AmlChecker(this.customerID).PreventSilentAutomation().Execute();
		} // DoAmlCheck

		private void DoBwaCheck(PreliminaryData preData) {
			bool shouldRunBwa =
				preData.AppBankAccountType == "Personal" &&
				preData.BwaBusinessCheck == "1" &&
				preData.AppSortCode != null &&
				preData.AppAccountNumber != null;

			if (shouldRunBwa)
				new BwaChecker(this.customerID).Execute();
		} // DoBwaCheck

		private void DoZooplaCheck() {
			new ZooplaStub(this.customerID).Execute();
		} // DoZooplaCheck

		private void UpdateLogicalGlue(PreliminaryData preData) {
			if (!this.logicalGlueEnabled) {
				Log.Debug("Not updating Logical Glue data for {0}: updating disabled.", OuterContextDescription);
				return;
			} // if

			if (preData.TypeOfBusiness.IsRegulated()) {
				Log.Debug("Not updating Logical Glue data for {0}: regulated company.", OuterContextDescription);
				return;
			} // if

			Log.Debug("Updating Logical Glue data for {0}: non-regulated company.", OuterContextDescription);

			try {
				InjectorStub.GetEngine().GetInference(
					this.customerID,
					this.monthlyPayment,
					false,
					GetInferenceMode.DownloadIfOld
				);
				Log.Debug("Updated Logical Glue data for customer {0}.", this.customerID);
			} catch (Exception e) {
				Log.Warn(e, "Logical Glue data was not updated for customer {0}.", this.customerID);
			} // try
		} // UpdateLogicalGlue

		private void ExecuteAdditionalStrategies() {
			var preData = new PreliminaryData(this.customerID);

			DoConsumerCheck(preData);
			DoCompanyCheck(preData);
			DoAmlCheck(preData);
			DoBwaCheck(preData);
			DoZooplaCheck();
			UpdateLogicalGlue(preData); // Must be after DoCompanyCheck because uses ExperianRefNum.
		} // ExecuteAdditionalStrategies

		private readonly int customerID;
		private readonly int marketplaceUpdateValidityDays;
		private readonly StrategiesMailer mailer;
		private readonly bool logicalGlueEnabled;
		private readonly int monthlyPayment;
	} // class UpdateData
} // namespace
