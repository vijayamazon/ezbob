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

			DataTable dt = DB.ExecuteReader("GetCompanyData", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
			var sr = new SafeReader(dt.Rows[0]);

			string companyType = sr["CompanyType"];
			companyNumber = sr["CompanyNumber"];

			isLimited = companyType == "Limited" || companyType == "LLP";
		} // constructor

		public override string Name {
			get { return "Experian company check"; }
		} // Name

		public override void Execute()
		{
			string experianError = null;
			decimal experianBureauScore = 0;

			if (string.IsNullOrEmpty(companyNumber))
			{
				experianError = "RefNumber is empty";
			}
			else
			{
				var service = new EBusinessService();
				BusinessReturnData experianData;
				if (isLimited)
				{
					experianData = service.GetLimitedBusinessData(companyNumber, customerId);
				}
				else
				{
					experianData = service.GetNotLimitedBusinessData(companyNumber, customerId);
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
					new QueryParameter("CompanyRefNumber", companyNumber),
					new QueryParameter("ExperianError", experianError),
					new QueryParameter("ExperianScore", experianBureauScore),
					new QueryParameter("CustomerId", customerId)
				);
		} // Execute

		private readonly int customerId;
		private readonly bool isLimited;
		private readonly string companyNumber;
	} // class ExperianCompanyCheck
} // namespace
