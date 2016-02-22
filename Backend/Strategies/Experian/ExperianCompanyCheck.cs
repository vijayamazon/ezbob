namespace Ezbob.Backend.Strategies.Experian {
	using System;
	using ConfigManager;
	using ExperianLib.Ebusiness;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation;
	using Ezbob.Backend.Strategies.CreditSafe;
	using Ezbob.Database;

	public class ExperianCompanyCheck : AStrategy {
		public ExperianCompanyCheck(int customerId, bool forceCheck) {
			this.doSilentAutomation = true;
			this.customerId = customerId;
			this.forceCheck = forceCheck;
			this.foundCompany = false;

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					this.foundCompany = true;

					string companyType = sr["CompanyType"];
					this.experianRefNum = sr["ExperianRefNum"];

					this.isLimited = companyType == "Limited" || companyType == "LLP";

					return ActionResult.SkipAll;
				},
				"GetCompanyData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
			);

			if (!this.foundCompany)
				Log.Info("Can't find company data for customer {0} (is the customer an entrepreneur?).", this.customerId);
		} // constructor

		public ExperianCompanyCheck PreventSilentAutomation() {
			this.doSilentAutomation = false;
			return this;
		} // PreventSilentAutomation

		public override string Name {
			get { return "Experian company check"; }
		} // Name

		public decimal MaxScore { get; private set; }

		public decimal Score { get; private set; }

		public static BusinessReturnData GetBusinessData(
			bool isLimited,
			string experianRefNum,
			int customerId,
			bool checkInCacheOnly,
			bool forceCheck
		) {
			var service = new EBusinessService(Library.Instance.DB);

			// Never run check ezbob company
			const string ezbobRefNum = "07852687";
			if (experianRefNum == ezbobRefNum)
				checkInCacheOnly = true;

			if (isLimited)
				return service.GetLimitedBusinessData(experianRefNum, customerId, checkInCacheOnly, forceCheck);

			return service.GetNotLimitedBusinessData(experianRefNum, customerId, checkInCacheOnly, forceCheck);
		} // GetBusinessData

		public override void Execute() {
			Log.Info(
				"Starting company check with parameters: IsLimited={0} ExperianRefNum={1}",
				this.isLimited ? "yes" : "no",
				this.experianRefNum
			);

			if (!this.foundCompany || (this.experianRefNum == "NotFound")) {
				Log.Info("Can't execute Experian company check for customer with no company");
				return;
			} // if

			string experianError = null;
			BusinessReturnData oExperianData = null;

			if (string.IsNullOrEmpty(this.experianRefNum))
				experianError = "RefNumber is empty";
			else {
				Log.Info("ExperianCompanyCheck strategy will make sure we have experian data");

				oExperianData = GetBusinessData(
					this.isLimited,
					this.experianRefNum,
					this.customerId,
					false,
					this.forceCheck
				);

				if (oExperianData != null) {
					Log.Info(
						"Fetched BureauScore {0} & MaxBureauScore {1} for customer {2}.",
						oExperianData.BureauScore,
						oExperianData.MaxBureauScore, this.customerId
					);

					if (!oExperianData.IsError) {
						MaxScore = oExperianData.MaxBureauScore;
						Score = oExperianData.BureauScore;
						Log.Info("Filled Score & MaxScore of the strategy");
					} else
						experianError = oExperianData.Error;

					if (!oExperianData.CacheHit && CurrentValues.Instance.CreditSafeEnabled) {
						try {
							AStrategy stra = this.isLimited
								? (AStrategy)new ServiceLogCreditSafeLtd(this.experianRefNum, this.customerId)
								: (AStrategy)new ServiceLogCreditSafeNonLtd(this.customerId);

							stra.Execute();
						} catch (Exception e) {
							Log.Error(e, "CreditSafeLtd/NonLtd NonCachHit failed for unexpected reason");
						} // try
					} // if
				} // if
			} // if

			if (!string.IsNullOrEmpty(experianError)) {
				Log.Warn(
					"Error in experian company check. Customer:{0} RefNumber:{1} Errors: {2}",
					this.customerId,
					this.experianRefNum,
					experianError
				);
			} // if

			Log.Info("Filling Analytics with Score: {0} & max score: {1}", Score, MaxScore);

			if (oExperianData == null) {
				Log.Debug("Premature completion: no data received from Experian.");
				return;
			} // if

			if (oExperianData.IsError) {
				Log.Debug("Premature completion because of error: {0}.", oExperianData.Error);
				return;
			} // if

			if (oExperianData.IsLimited)
				new UpdateLimitedExperianDirectors(this.customerId, oExperianData.ServiceLogID).Execute();

			if (this.doSilentAutomation)
				new SilentAutomation(this.customerId).SetTag(SilentAutomation.Callers.Company).Execute();
		} // Execute

		private readonly int customerId;
		private readonly bool forceCheck;
		private bool foundCompany;
		private bool isLimited;
		private string experianRefNum;
		private bool doSilentAutomation;
	} // class ExperianCompanyCheck
} // namespace
