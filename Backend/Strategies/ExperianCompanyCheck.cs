namespace EzBob.Backend.Strategies {
	using ExperianLib.Ebusiness;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ExperianCompanyCheck : AStrategy {
		public ExperianCompanyCheck(int customerId, bool isLimited, string companyRefNumber, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
			this.isLimited = isLimited;
			this.companyRefNumber = companyRefNumber;
		} // constructor

		public override string Name {
			get { return "Experian company check"; }
		} // Name

		public override void Execute()
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
		} // Execute

		private readonly int customerId;
		private readonly bool isLimited;
		private readonly string companyRefNumber;
	} // class ExperianCompanyCheck
} // namespace
