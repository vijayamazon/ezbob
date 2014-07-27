namespace EzBob.Backend.Strategies.Misc 
{
	using System;
	using System.Data;
	using ExperianLib.IdIdentityHub;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Newtonsoft.Json;

	public class BackfillAml : AStrategy
	{
		private readonly IdHubService idHubService = new IdHubService();

		public BackfillAml(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
		}

		public override string Name {
			get { return "BackfillAml"; }
		}

		public override void Execute()
		{
			DataTable dt = DB.ExecuteReader(
				"GetDataForAmlBackfill",
				CommandSpecies.StoredProcedure
				);

			int customerId = 0;
			foreach (DataRow row in dt.Rows)
			{
				try
				{
					var sr = new SafeReader(row);
					customerId = sr["CustomerId"];
					string existingAmlResult = sr["AmlResult"];
					string xml = sr["xml"];

					Log.Debug("Backfilling aml in customer {0}", customerId);

					var result = idHubService.GetResults_ForBackfill(xml);

					int authentication = (int)result.AuthenticationIndex;
					string description = result.AuthIndexText;

					DB.ExecuteNonQuery("UpdateAmlResult", CommandSpecies.StoredProcedure,
						new QueryParameter("CustomerId", customerId),
						new QueryParameter("AmlResult", existingAmlResult),
						new QueryParameter("AmlScore", authentication),
						new QueryParameter("AmlDescription", description)
					);
				}
				catch (Exception ex)
				{
					Log.Error("The backfill for customer:{0} failed with exception:{2}", customerId, ex);
				}
			}
		}
	}
}
