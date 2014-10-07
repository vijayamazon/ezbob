namespace EzBob.Backend.Strategies.Experian {
	using ExperianLib.Ebusiness;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BackfillCustomerAnalyticsCompany : AStrategy {
		#region public

		#region constructor

		public BackfillCustomerAnalyticsCompany(AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog) {
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "BackfillCustomerAnalyticsCompany"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			var service = new EBusinessService(DB);

			foreach (SafeReader sr in DB.ExecuteEnumerable("LoadDataForBackfillCustomerAnalyticsCompany", CommandSpecies.StoredProcedure)) {
				string sCompanyRegNum = sr["CompanyRegNum"];
				int nCustomerID = sr["CustomerID"];

				Log.Debug("Backfill for company {0} of customer {1}...", sCompanyRegNum, nCustomerID);

				LimitedResults oData = service.GetLimitedBusinessData(sCompanyRegNum, nCustomerID, true, false);

				if (oData == null) {
					Log.Debug("Backfill for company {0} of customer {1}: no data.", sCompanyRegNum, nCustomerID);
					continue;
				} // if

				Log.Debug("Backfill for company {0} of customer {1}: data found.", sCompanyRegNum, nCustomerID);

				ExperianCompanyCheck.UpdateAnalyticsForLimited(oData.MaxBureauScore, oData, nCustomerID, DB, Log);

				Log.Debug("Backfill for company {0} of customer {1}: analytics updated.", sCompanyRegNum, nCustomerID);
			} // for each row
		} // Execute

		#endregion method Execute

		#endregion public

		#region private
		#endregion private
	} // class BackfillCustomerAnalyticsCompany
} // namespace
