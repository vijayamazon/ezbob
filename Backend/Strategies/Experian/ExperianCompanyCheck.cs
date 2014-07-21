namespace EzBob.Backend.Strategies.Experian {
	using ExperianLib.Ebusiness;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ExperianCompanyCheck : AStrategy {
		#region public

		#region constructor

		public ExperianCompanyCheck(int nCustomerID, bool bForceCheck, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oExperianData = null;
			m_nCustomerID = nCustomerID;
			m_bForceCheck = bForceCheck;
			m_bFoundCompany = false;

			m_oParser = new ExperianParserForAnalytics(DB, Log);

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

			if (string.IsNullOrEmpty(m_sExperianRefNum))
				experianError = "RefNumber is empty";
			else {
				Log.Info("ExperianCompanyCheck strategy will make sure we have experian data");

				m_oExperianData = GetBusinessDataFromExperian();

				Log.Info("Fetched BureauScore {0} & MaxBureauScore {1} for customer {2}.", m_oExperianData.BureauScore, m_oExperianData.MaxBureauScore, m_nCustomerID);

				if (!m_oExperianData.IsError) {
					MaxScore = m_oExperianData.MaxBureauScore;
					Score = m_oExperianData.BureauScore;
					Log.Info("Filled Score & MaxScore of the strategy");
				}
				else
					experianError = m_oExperianData.Error;
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

			if (m_oExperianData == null) {
				Log.Debug("Premature completion: no data received from Experian.");
				return;
			} // if

			if (m_oExperianData.IsError) {
				Log.Debug("Premature completion because of error: {0}.", m_oExperianData.Error);
				return;
			} // if

			if (!m_oExperianData.CacheHit)
				new UpdateExperianDirectors(m_nCustomerID, m_oExperianData.ServiceLogID, m_oExperianData.IsLimited ? string.Empty : m_oExperianData.OutputXml, m_oExperianData.IsLimited, DB, Log).Execute();

			if (m_oExperianData.CacheHit) {
				// This check is required to allow multiple customers have the same company
				// While the cache works with RefNumber the analytics table works with customer
				if (IsCustomerAlreadyInAnalytics())
					return;
			} // if

			if (m_oExperianData.IsLimited)
			{
				m_oParser.UpdateAnalytics(m_nCustomerID, (int) MaxScore);
			}
			else
			{
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
		private BusinessReturnData m_oExperianData;
		private readonly ExperianParserForAnalytics m_oParser;

		#endregion fields

		#endregion private
	} // class ExperianCompanyCheck
} // namespace
