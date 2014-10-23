namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Linq;
	using ExperianLib.Ebusiness;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ExperianCompanyCheck : AStrategy {
		private readonly int customerId;
		private bool foundCompany;
		private readonly bool forceCheck;
		private bool isLimited;
		private string experianRefNum;

		public ExperianCompanyCheck(int customerId, bool forceCheck, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog) {
			this.customerId = customerId;
			this.forceCheck = forceCheck;
			foundCompany = false;

			oDb.ForEachRowSafe(
				(sr, bRowsetStart) => {
					foundCompany = true;

					string companyType = sr["CompanyType"];
					experianRefNum = sr["ExperianRefNum"];

					isLimited = companyType == "Limited" || companyType == "LLP";

					return ActionResult.SkipAll;
				},
				"GetCompanyData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", this.customerId)
			);

			if (!foundCompany)
				oLog.Info("Can't find company data for customer {0} (is the customer an entrepreneur?).", this.customerId);
		}

		public override string Name {
			get { return "Experian company check"; }
		}

		public decimal MaxScore { get; private set; }
		public decimal Score { get; private set; }

		public override void Execute() {
			Log.Info("Starting company check with parameters: IsLimited={0} ExperianRefNum={1}", isLimited ? "yes" : "no", experianRefNum);

			if (!foundCompany || (experianRefNum == "NotFound")) {
				Log.Info("Can't execute Experian company check for customer with no company");
				return;
			} // if

			string experianError = null;
			BusinessReturnData oExperianData = null;

			if (string.IsNullOrEmpty(experianRefNum))
				experianError = "RefNumber is empty";
			else {
				Log.Info("ExperianCompanyCheck strategy will make sure we have experian data");

				var service = new EBusinessService(DB);

				// ReSharper disable RedundantCast
				oExperianData = isLimited
					? (BusinessReturnData)service.GetLimitedBusinessData(experianRefNum, customerId, false, forceCheck)
					: (BusinessReturnData)service.GetNotLimitedBusinessData(experianRefNum, customerId, false, forceCheck);
				// ReSharper restore RedundantCast

				if (oExperianData != null) {
					Log.Info(
						"Fetched BureauScore {0} & MaxBureauScore {1} for customer {2}.",
						oExperianData.BureauScore,
						oExperianData.MaxBureauScore,
						customerId
						);

					if (!oExperianData.IsError) {
						MaxScore = oExperianData.MaxBureauScore;
						Score = oExperianData.BureauScore;
						Log.Info("Filled Score & MaxScore of the strategy");
					}
					else
						experianError = oExperianData.Error;
				} // if
			} // if

			if (!string.IsNullOrEmpty(experianError))
				Log.Error("Error in experian company check. Customer:{0} RefNumber:{1} Errors: {2}", customerId, experianRefNum, experianError);

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
				new UpdateLimitedExperianDirectors(customerId, oExperianData.ServiceLogID, DB, Log).Execute();

			if (oExperianData.CacheHit) {
				// This check is required to allow multiple customers have the same company
				// While the cache works with RefNumber the analytics table works with customer
				if (IsCustomerAlreadyInAnalytics())
					return;
			} // if

			if (oExperianData.IsLimited)
				UpdateAnalyticsForLimited(MaxScore, (LimitedResults)oExperianData, customerId, DB, Log);
			else {
				DB.ExecuteNonQuery(
					"CustomerAnalyticsUpdateNonLimitedCompany",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId),
					new QueryParameter("RefNumber", experianRefNum),
					new QueryParameter("MaxScore", MaxScore)
				);
			} // if
		} // Execute

		private bool IsCustomerAlreadyInAnalytics() {
			return DB.ExecuteScalar<bool>(
				"CustomerHasCompanyAnalytics",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId)
			);
		}

		public static void UpdateAnalyticsForLimited(decimal nMaxScore, LimitedResults oExperianData, int nCustomerID, AConnection oDB, ASafeLog oLog) {
			ExperianLtd oExperianLtd = oExperianData.RawExperianLtd;

			oLog.Debug("Updating limited customer analytics for customer {0} and company '{1}'...", nCustomerID, oExperianLtd.RegisteredNumber);

			decimal tangibleEquity = 0;
			decimal adjustedProfit = 0;

			ExperianLtdDL99[] ary = oExperianLtd.GetChildren<ExperianLtdDL99>().Where(x => x.Date.HasValue).ToArray();

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
				}
			}

			oLog.Info("Inserting to analytics Experian Score: {0} MaxScore: {1}.", oExperianLtd.GetCommercialDelphiScore(), nMaxScore);

			oDB.ExecuteNonQuery(
				"CustomerAnalyticsUpdateCompany",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", nCustomerID),
				new QueryParameter("Score", oExperianLtd.GetCommercialDelphiScore()),
				new QueryParameter("MaxScore", (int)nMaxScore),
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

			oLog.Debug("Updating limited customer analytics for customer {0} and company '{1}' complete.", nCustomerID, oExperianLtd.RegisteredNumber);
		} // UpdateAnalyticsDataForLimited
	}
}
