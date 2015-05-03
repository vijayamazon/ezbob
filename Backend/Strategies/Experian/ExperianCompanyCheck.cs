namespace Ezbob.Backend.Strategies.Experian {
	using System;
	using System.Linq;
	using ExperianLib.Ebusiness;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation;
	using Ezbob.Backend.Strategies.CreditSafe;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ExperianCompanyCheck : AStrategy {
		public ExperianCompanyCheck(int customerId, bool forceCheck) {
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

		public override string Name {
			get { return "Experian company check"; }
		} // Name

		public decimal MaxScore { get; private set; }

		public decimal Score { get; private set; }

		public static void UpdateAnalyticsForNonLimited(decimal maxScore, string experianRefNum, int customerID) {
			Library.Instance.DB.ExecuteNonQuery(
				"CustomerAnalyticsUpdateNonLimitedCompany",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerID),
				new QueryParameter("RefNumber", experianRefNum),
				new QueryParameter("MaxScore", maxScore)
			);
		} // UpdateAnalyticsForNonLimited

		public static void UpdateAnalyticsForLimited(
			LimitedResults oExperianData,
			int nCustomerID
		) {
			AConnection oDB = Library.Instance.DB;
			ASafeLog oLog = Library.Instance.Log;

			ExperianLtd oExperianLtd = oExperianData.RawExperianLtd;

			oLog.Debug(
				"Updating limited customer analytics for customer {0} and company '{1}'...",
				nCustomerID,
				oExperianLtd.RegisteredNumber
				);

			decimal tangibleEquity = 0;
			decimal adjustedProfit = 0;

			ExperianLtdDL99[] ary = oExperianLtd.GetChildren<ExperianLtdDL99>()
				.Where(x => x.Date.HasValue)
				.ToArray();

			// ReSharper disable PossibleInvalidOperationException
			Array.Sort(ary, (a, b) => b.Date.Value.CompareTo(a.Date.Value));
			// ReSharper restore PossibleInvalidOperationException

			if (ary.Length > 0) {
				decimal totalShareFund = ary[0].TotalShareFund ?? 0;
				decimal inTngblAssets = ary[0].InTngblAssets ?? 0;
				decimal debtorsDirLoans = ary[0].DebtorsDirLoans ?? 0;
				decimal credDirLoans = ary[0].CredDirLoans ?? 0;
				decimal onClDirLoans = ary[0].OnClDirLoans ?? 0;

				tangibleEquity = totalShareFund - inTngblAssets - debtorsDirLoans + credDirLoans + onClDirLoans;

				if (ary.Length > 1) {
					decimal retainedEarnings = ary[0].RetainedEarnings ?? 0;
					decimal retainedEarningsPrev = ary[1].RetainedEarnings ?? 0;
					decimal fixedAssetsPrev = ary[1].TngblAssets ?? 0;

					adjustedProfit = retainedEarnings - retainedEarningsPrev + fixedAssetsPrev / 5;
				} // if
			} // if

			oLog.Info(
				"Inserting to analytics Experian Score: {0} MaxScore: {1}.",
				oExperianLtd.GetCommercialDelphiScore(),
				oExperianData.MaxBureauScore
			);

			oDB.ExecuteNonQuery(
				"CustomerAnalyticsUpdateCompany",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", nCustomerID),
				new QueryParameter("Score", oExperianLtd.GetCommercialDelphiScore()),
				new QueryParameter("MaxScore", (int)oExperianData.MaxBureauScore),
				new QueryParameter("SuggestedAmount", oExperianLtd.GetCommercialDelphiCreditLimit()),
				new QueryParameter("IncorporationDate", oExperianLtd.IncorporationDate),
				new QueryParameter("TangibleEquity", tangibleEquity),
				new QueryParameter("AdjustedProfit", adjustedProfit),
				new QueryParameter("Sic1980Code1", oExperianLtd.First1980SICCode),
				new QueryParameter("Sic1980Desc1", oExperianLtd.First1980SICCodeDescription),
				new QueryParameter("Sic1992Code1", oExperianLtd.First1992SICCode),
				new QueryParameter("Sic1992Desc1", oExperianLtd.First1992SICCodeDescription),
				new QueryParameter("AgeOfMostRecentCcj", oExperianLtd.GetAgeOfMostRecentCCJDecreeMonths()),
				new QueryParameter("NumOfCcjsInLast24Months", oExperianLtd.GetNumberOfCcjsInLast24Months()),
				new QueryParameter("SumOfCcjsInLast24Months", oExperianLtd.GetSumOfCcjsInLast24Months()),
				new QueryParameter("AnalyticsDate", DateTime.UtcNow)
			);

			oLog.Debug(
				"Updating limited customer analytics for customer {0} and company '{1}' complete.",
				nCustomerID,
				oExperianLtd.RegisteredNumber
			);
		} // UpdateAnalyticsDataForLimited

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

				    if (!oExperianData.CacheHit) {
				        if (this.isLimited) {
                            ServiceLogCreditSafeLtd LtdStra = new ServiceLogCreditSafeLtd(this.experianRefNum,this.customerId);
                            LtdStra.Execute();
				        } else {
				            ServiceLogCreditSafeNonLtd NonLtdStra = new ServiceLogCreditSafeNonLtd(this.customerId);
                            NonLtdStra.Execute();
				        }
				    }
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

			if (oExperianData.CacheHit) {
				// This check is required to allow multiple customers have the same company
				// While the cache works with RefNumber the analytics table works with customer
				if (IsCustomerAlreadyInAnalytics())
					return;
			} // if

			if (oExperianData.IsLimited)
				UpdateAnalyticsForLimited((LimitedResults)oExperianData, this.customerId);
			else
				UpdateAnalyticsForNonLimited(MaxScore, this.experianRefNum, this.customerId);

			new SilentAutomation(this.customerId).SetTag(SilentAutomation.Callers.Company).Execute();
		} // Execute

		private bool IsCustomerAlreadyInAnalytics() {
			return DB.ExecuteScalar<bool>(
				"CustomerHasCompanyAnalytics",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
			);
		} // IsCustomerAlreadyInAnalytics

		private readonly int customerId;
		private readonly bool forceCheck;
		private bool foundCompany;
		private bool isLimited;
		private string experianRefNum;
	} // class ExperianCompanyCheck
} // namespace
