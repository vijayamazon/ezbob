namespace EzBob.Backend.Strategies 
{
	using System;
	using System.Data;
	using EZBob.DatabaseLib.Model.Database;
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
				MP_ExperianDataCache tmp = businessService.Temp_CheckCache(refNumber);
				if (tmp != null)
				{
					int currentBalanceSum = businessService.ParseToDb(tmp);
					var limitedResults = new LimitedResults(tmp.JsonPacket, DateTime.UtcNow)
						{
							CacheHit = false,
							CurrentBalanceSum = currentBalanceSum
						};

					Log.Debug("Backfilling customer analytics for customer {0} and company '{1}'...", customerId, refNumber);

					DB.ExecuteNonQuery(
						"CustomerAnalyticsUpdateCompany",
						CommandSpecies.StoredProcedure,
						new QueryParameter("CustomerID", customerId),
						new QueryParameter("Score", limitedResults.BureauScore),
						new QueryParameter("SuggestedAmount", limitedResults.CreditLimit),
						new QueryParameter("IncorporationDate", limitedResults.IncorporationDate),
						new QueryParameter("AnalyticsDate", DateTime.UtcNow),
						new QueryParameter("CurrentBalanceSum", limitedResults.CurrentBalanceSum ?? 0)
						);

					Log.Debug("Backfilling customer analytics for customer {0} and company '{1}' complete.", customerId, refNumber);
				}
			}
		}
	}
}
