﻿namespace EzBob.Backend.Strategies.Experian {
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

		#region method Execute

		public override void Execute() {
			Log.Info("Starting company check with parameters: IsLimited={0} ExperianRefNum={1}", m_bIsLimited ? "yes" : "no", m_sExperianRefNum);

			if (!m_bFoundCompany || (m_sExperianRefNum == "NotFound")) {
				Log.Info("Can't execute Experian company check for customer with no company");
				return;
			} // if

			string experianError = null;
			decimal experianBureauScore = 0;

			if (string.IsNullOrEmpty(m_sExperianRefNum))
				experianError = "RefNumber is empty";
			else {
				m_oExperianData = GetBusinessDataFromExperian();

				if (!m_oExperianData.IsError) {
					experianBureauScore = m_oExperianData.BureauScore;
					MaxScore = m_oExperianData.MaxBureauScore;
				}
				else
					experianError = m_oExperianData.Error;
			} // if

			DB.ExecuteNonQuery(
				"UpdateExperianBusiness",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CompanyRefNumber", m_sExperianRefNum),
				new QueryParameter("ExperianError", experianError),
				new QueryParameter("ExperianScore", experianBureauScore),
				new QueryParameter("ExperianMaxScore", MaxScore),
				new QueryParameter("CustomerId", m_nCustomerID)
			);

			if ((m_oExperianData == null) || m_oExperianData.IsError)
				return;

			if (m_oExperianData.CacheHit) {
				// This check is required to allow multiple customers have the same company
				// While the cache works with RefNumber the analytics table works with customer
				if (IsCustomerAlreadyInAnalytics())
					return;
			} // if

			m_oParser.UpdateAnalytics(m_nCustomerID);
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
			var service = new EBusinessService();

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
