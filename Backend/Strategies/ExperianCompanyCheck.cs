namespace EzBob.Backend.Strategies {
	using System.Data;
	using ExperianLib.Ebusiness;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ExperianCompanyCheck : AStrategy {
		public ExperianCompanyCheck(int customerId, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
		} // constructor

		public override string Name {
			get { return "Experian company check"; }
		} // Name

		public override void Execute() {
			if (companyType == "Entrepreneur")
			{
				Log.Info("Skipping experian company check for customer:{0} because he is an entrepreneur", customerId);
				return;
			}

			DataTable dt = DB.ExecuteReader("GetCompanyRefNumbers", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
			var results = new SafeReader(dt.Rows[0]);

			companyType = results["CompanyType"];
			if (companyType == "Entrepreneur")
			{
				Log.Info("Skipping experian company check for customer:{0} because he is an entrepreneur", customerId);
				return;
			}

			isLimited = companyType == "Limited" || companyType == "LLP";
			companyRefNumber = isLimited ? results["LimitedRefNum"] : results["NonLimitedRefNum"];

			UpdateCompanyScore();
		} // Execute
		
		private void UpdateCompanyScore()
		{
			string experianError = null;
			decimal experianBureauScore = 0;

			if (string.IsNullOrEmpty(companyRefNumber))
			{
				experianError = "RefNumber is empty";
			}
			else
			{
				var service = new EBusinessService();
				BusinessReturnData experianData;
				if (isLimited)
				{
					experianData = service.GetLimitedBusinessData(companyRefNumber, customerId);
				}
				else
				{
					experianData = service.GetNotLimitedBusinessData(companyRefNumber, customerId); 
				}

				if (!experianData.IsError)
				{
					experianBureauScore = experianData.BureauScore;
				}
				else
				{
					experianError = experianData.Error;
				}
			} // if

			DB.ExecuteNonQuery(
					"UpdateExperianBusiness",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CompanyRefNumber", companyRefNumber),
					new QueryParameter("ExperianError", experianError),
					new QueryParameter("ExperianScore", experianBureauScore),
					new QueryParameter("CustomerId", customerId)
				);
		}

		private readonly int customerId;
		private bool isLimited;
		private string companyType;
		private string companyRefNumber;
	} // class ExperianCompanyCheck
} // namespace
