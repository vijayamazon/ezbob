namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Data;
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

			DB.ForEachRowSafe(
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
				Log.Info("Can't find company data for customer:{0}. The customer is probably an entrepreneur.");
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Experian company check"; }
		} // Name

		#endregion property Name

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

				if (!m_oExperianData.IsError)
					experianBureauScore = m_oExperianData.BureauScore;
				else
					experianError = m_oExperianData.Error;
			} // if

			DB.ExecuteNonQuery(
				"UpdateExperianBusiness",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CompanyRefNumber", m_sExperianRefNum),
				new QueryParameter("ExperianError", experianError),
				new QueryParameter("ExperianScore", experianBureauScore),
				new QueryParameter("CustomerId", m_nCustomerID)
			);

			UpdateAnalytics();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		#region method GetBusinessDataFromExperian

		private BusinessReturnData GetBusinessDataFromExperian() {
			var service = new EBusinessService();

			if (m_bIsLimited)
				return service.GetLimitedBusinessData(m_sExperianRefNum, m_nCustomerID, false, m_bForceCheck);

			return service.GetNotLimitedBusinessData(m_sExperianRefNum, m_nCustomerID, false, m_bForceCheck);
		} // GetBusinessDataFromExperian

		#endregion method GetBusinessDataFromExperian

		#region method UpdateAnalytics

		private void UpdateAnalytics() {
			if ((m_oExperianData == null) || m_oExperianData.IsError)
				return;

			if (m_oExperianData.CacheHit)
			{
				// This check is required to allow multiple customers have the same company
				// While the cache works with RefNumber the analytics table works with customer
				if (IsCustomerAlreadyInAnalytics())
				{
					return;
				}
			}

			Log.Debug("Updating customer analytics for customer {0} and company '{1}'...", m_nCustomerID, m_sExperianRefNum);

			DB.ExecuteNonQuery(
				"CustomerAnalyticsUpdateCompany",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", m_nCustomerID),
				new QueryParameter("Score", m_oExperianData.BureauScore),
				new QueryParameter("SuggestedAmount", m_oExperianData.CreditLimit),
				new QueryParameter("IncorporationDate", m_oExperianData.IncorporationDate),
				new QueryParameter("AnalyticsDate", DateTime.UtcNow),
				new QueryParameter("CurrentBalanceSum", m_oExperianData.CurrentBalanceSum ?? 0)
			);

			Log.Debug("Updating customer analytics for customer {0} and company '{1}' complete.", m_nCustomerID, m_sExperianRefNum);
		} // UpdateAnalytics

		#endregion method UpdateAnalytics
		
		private bool IsCustomerAlreadyInAnalytics()
		{
			DataTable dt = DB.ExecuteReader(
				"GetCompanyScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", m_nCustomerID)
			);

			if (dt.Rows.Count == 1)
			{
				return true;
			}

			return false;
		}

		#region fields

		private readonly int m_nCustomerID;
		private bool m_bFoundCompany;
		private readonly bool m_bForceCheck;
		private bool m_bIsLimited;
		private string m_sExperianRefNum;
		private BusinessReturnData m_oExperianData;

		#endregion fields

		#endregion private
	} // class ExperianCompanyCheck
} // namespace
