namespace EzBob.Backend.Strategies 
{
	using System;
	using System.Data;
	using ExperianLib.Ebusiness;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BackfillCompanyAnalytics : AStrategy
	{
		public BackfillCompanyAnalytics(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
		}

		public override string Name {
			get { return "BackfillCompanyAnalytics"; }
		}

		public override void Execute()
		{
			var businessService = new EBusinessService();
			DataTable dt = DB.ExecuteReader(
				"GetAllCustomersWithCompany",
				CommandSpecies.StoredProcedure
				);

			int customerId = 0;
			foreach (DataRow row in dt.Rows)
			{
				try
				{
					var sr = new SafeReader(row);
					customerId = sr["CustomerId"];
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
				catch (Exception ex)
				{
					Log.Error("The backfill for customer:{0} failed with exception:{1}", customerId, ex);
				}
			}
		}
	}
}
