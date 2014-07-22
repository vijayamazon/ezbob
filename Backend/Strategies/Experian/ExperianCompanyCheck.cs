namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Linq;
	using ExperianLib.Ebusiness;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ExperianCompanyCheck : AStrategy {
		#region public

		#region constructor

		public ExperianCompanyCheck(int nCustomerID, bool bForceCheck, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
			m_bForceCheck = bForceCheck;
			m_bFoundCompany = false;

			oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					m_bFoundCompany = true;

					string companyType = sr["CompanyType"];
					m_sExperianRefNum = sr["ExperianRefNum"];

					m_bIsLimited = companyType == "Limited" || companyType == "LLP";

					return ActionResult.SkipAll;
				},
				"GetCompanyData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", m_nCustomerID)
			);

			if (!m_bFoundCompany)
				oLog.Info("Can't find company data for customer {0} (is the customer an entrepreneur?).", m_nCustomerID);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Experian company check"; }
		} // Name

		#endregion property Name

		public decimal MaxScore { get; private set; }
		public decimal Score { get; private set; }

		#region method Execute

		public override void Execute() {
			Log.Info("Starting company check with parameters: IsLimited={0} ExperianRefNum={1}", m_bIsLimited ? "yes" : "no", m_sExperianRefNum);

			if (!m_bFoundCompany || (m_sExperianRefNum == "NotFound")) {
				Log.Info("Can't execute Experian company check for customer with no company");
				return;
			} // if

			string experianError = null;
			BusinessReturnData oExperianData = null;

			if (string.IsNullOrEmpty(m_sExperianRefNum))
				experianError = "RefNumber is empty";
			else {
				Log.Info("ExperianCompanyCheck strategy will make sure we have experian data");

				oExperianData = GetBusinessDataFromExperian();

				Log.Info("Fetched BureauScore {0} & MaxBureauScore {1} for customer {2}.", oExperianData.BureauScore, oExperianData.MaxBureauScore, m_nCustomerID);

				if (!oExperianData.IsError) {
					MaxScore = oExperianData.MaxBureauScore;
					Score = oExperianData.BureauScore;
					Log.Info("Filled Score & MaxScore of the strategy");
				}
				else
					experianError = oExperianData.Error;
			} // if

			Log.Info("Filling Analytics with Score: {0} & max score: {1}", Score, MaxScore);

			DB.ExecuteNonQuery(
				"UpdateExperianBusiness",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CompanyRefNumber", m_sExperianRefNum),
				new QueryParameter("ExperianError", experianError),
				new QueryParameter("ExperianScore", Score),
				new QueryParameter("ExperianMaxScore", MaxScore),
				new QueryParameter("CustomerId", m_nCustomerID)
			);

			if (oExperianData == null) {
				Log.Debug("Premature completion: no data received from Experian.");
				return;
			} // if

			if (oExperianData.IsError) {
				Log.Debug("Premature completion because of error: {0}.", oExperianData.Error);
				return;
			} // if

			if (!oExperianData.CacheHit)
				new UpdateExperianDirectors(m_nCustomerID, oExperianData.ServiceLogID, oExperianData.IsLimited ? string.Empty : oExperianData.OutputXml, oExperianData.IsLimited, DB, Log).Execute();

			if (oExperianData.CacheHit) {
				// This check is required to allow multiple customers have the same company
				// While the cache works with RefNumber the analytics table works with customer
				if (IsCustomerAlreadyInAnalytics())
					return;
			} // if

			if (oExperianData.IsLimited)
				UpdateAnalyticsForLimited(MaxScore, (LimitedResults)oExperianData);
			else {
				DB.ExecuteNonQuery(
					"CustomerAnalyticsUpdateNonLimitedCompany",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", m_nCustomerID),
					new QueryParameter("RefNumber", m_sExperianRefNum),
					new QueryParameter("MaxScore", MaxScore)
				);
			}
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		#region method IsCustomerAlreadyInAnalytics

		private bool IsCustomerAlreadyInAnalytics() {
			return DB.ExecuteScalar<bool>(
				"CustomerHasCompanyAnalytics",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", m_nCustomerID)
			);
		} // IsCustomerAlreadyInAnalytics

		#endregion method IsCustomerAlreadyInAnalytics

		#region method GetBusinessDataFromExperian
		// ReSharper disable RedundantCast

		private BusinessReturnData GetBusinessDataFromExperian() {
			var service = new EBusinessService(DB);

			return m_bIsLimited
				? (BusinessReturnData)service.GetLimitedBusinessData(m_sExperianRefNum, m_nCustomerID, false, m_bForceCheck)
				: (BusinessReturnData)service.GetNotLimitedBusinessData(m_sExperianRefNum, m_nCustomerID, false, m_bForceCheck);
		} // GetBusinessDataFromExperian

		// ReSharper restore RedundantCast
		#endregion method GetBusinessDataFromExperian

		#region fields

		private readonly int m_nCustomerID;
		private bool m_bFoundCompany;
		private readonly bool m_bForceCheck;
		private bool m_bIsLimited;
		private string m_sExperianRefNum;

		#endregion fields

		#region update analytics

		#region method UpdateAnalyticsForLimited

		private void UpdateAnalyticsForLimited(decimal nMaxScore, LimitedResults oExperianData) {
			ExperianLtd oExperianLtd = oExperianData.RawExperianLtd;

			Log.Debug("Updating limited customer analytics for customer {0} and company '{1}'...", m_nCustomerID, oExperianLtd.RegisteredNumber);

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
				} // if
			} // if

			Log.Info("Inserting to analytics Experian Score: {0} MaxScore: {1}.", oExperianLtd.GetCommercialDelphiScore(), nMaxScore);

			DB.ExecuteNonQuery(
				"CustomerAnalyticsUpdateCompany",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", m_nCustomerID),
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

			Log.Debug("Updating limited customer analytics for customer {0} and company '{1}' complete.", m_nCustomerID, oExperianLtd.RegisteredNumber);
		} // UpdateAnalyticsForLimited

		#endregion method UpdateAnalyticsForLimited

		#endregion update analytics

		#endregion private
	} // class ExperianCompanyCheck
} // namespace
