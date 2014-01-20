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

			if (dt.Rows.Count == 0)
			{
				Log.Info("Can't find company data for customer:{0}. The customer is probably an entrepreneur");
				return;
			}

			foundCompany = true;

			var sr = new SafeReader(dt.Rows[0]);

			string companyType = sr["CompanyType"];
			experianRefNum = sr["ExperianRefNum"];

			isLimited = companyType == "Limited" || companyType == "LLP";
		} // constructor

		public override string Name {
			get { return "Experian company check"; }
		} // Name

		public override void Execute()
		{
			Log.Info("Starting company check with params: IsLimited={0} ExperianRefNum={1}", isLimited, experianRefNum);
			if (!foundCompany)
			{
				Log.Info("Can't execute Experian company check for customer with no company");
				return;
			}

			string experianError = null;
			decimal experianBureauScore = 0;

			if (string.IsNullOrEmpty(experianRefNum))
			{
				experianError = "RefNumber is empty";
			}
			else
			{
				var service = new EBusinessService();
				BusinessReturnData experianData;
				if (isLimited)
				{
					experianData = service.GetLimitedBusinessData(experianRefNum, customerId);
				}
				else
				{
					experianData = service.GetNotLimitedBusinessData(experianRefNum, customerId);
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
					new QueryParameter("CompanyRefNumber", experianRefNum),
					new QueryParameter("ExperianError", experianError),
					new QueryParameter("ExperianScore", experianBureauScore),
					new QueryParameter("CustomerId", customerId)
				);
		} // Execute

		private readonly int customerId;
		private readonly bool foundCompany;
		private readonly bool isLimited;
		private readonly string experianRefNum;
	} // class ExperianCompanyCheck
} // namespace
