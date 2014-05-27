namespace EzBob.Backend.Strategies 
{
	using System;
	using System.Data;
	using Experian;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BackfillCompanyAnalytics : AStrategy
	{
		private readonly ExperianParserForAnalytics experianParserForAnalytics;

		public BackfillCompanyAnalytics(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			experianParserForAnalytics = new ExperianParserForAnalytics(oLog, oDb);
		}

		public override string Name {
			get { return "BackfillCompanyAnalytics"; }
		}

		public override void Execute()
		{
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

					experianParserForAnalytics.UpdateAnalytics(customerId);
				}
				catch (Exception ex)
				{
					Log.Error("The backfill for customer:{0} failed with exception:{1}", customerId, ex);
				}
			}
		}
	}
}
