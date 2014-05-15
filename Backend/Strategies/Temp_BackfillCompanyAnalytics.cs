namespace EzBob.Backend.Strategies 
{
	using System;
	using System.Data;
	using ExperianLib.Ebusiness;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class Temp_BackfillCompanyAnalytics : AStrategy
	{
		public Temp_BackfillCompanyAnalytics(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
		}

		public override string Name {
			get { return "Temp_BackfillCompanyAnalytics"; }
		}

		public override void Execute()
		{
			var businessService = new EBusinessService();
			DataTable dt = DB.ExecuteReader(
				"Temp_GetAllCustomersWithCompany",
				CommandSpecies.StoredProcedure
				);

			foreach (DataRow row in dt.Rows)
			{
				var sr = new SafeReader(row);
				int customerId = sr["CustomerId"];
				string refNumber = sr["RefNumber"];
				string typeOfBusiness = sr["TypeOfBusiness"];
				string response = sr["Response"];
				BusinessReturnData experianResults;

				if (typeOfBusiness == "Limited" || typeOfBusiness == "LLP")
				{
					businessService.ParseToDb(customerId, response);
					experianResults = new LimitedResults(response, DateTime.UtcNow)
					{
						CacheHit = false
					};
				}
				else
				{
					experianResults = new NonLimitedResults(response, DateTime.UtcNow)
					{
						CacheHit = false
					};
				}

				Log.Debug("Backfilling customer analytics for customer {0} and company '{1}'...", customerId, refNumber);

				DB.ExecuteNonQuery(
					"CustomerAnalyticsUpdateCompany",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerID", customerId),
					new QueryParameter("Score", experianResults.BureauScore),
					new QueryParameter("SuggestedAmount", experianResults.CreditLimit),
					new QueryParameter("IncorporationDate", experianResults.IncorporationDate),
					new QueryParameter("AnalyticsDate", DateTime.UtcNow));

				Log.Debug("Backfilling customer analytics for customer {0} and company '{1}' complete.", customerId, refNumber);
			}
		}
	}
}
