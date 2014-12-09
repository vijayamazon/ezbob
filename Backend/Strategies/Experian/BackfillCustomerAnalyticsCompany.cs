namespace Ezbob.Backend.Strategies.Experian {
	using ExperianLib.Ebusiness;
	using Ezbob.Database;

	public class BackfillCustomerAnalyticsCompany : AStrategy {
		public override string Name {
			get { return "BackfillCustomerAnalyticsCompany"; }
		} // Name

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

	} // class BackfillCustomerAnalyticsCompany
} // namespace
