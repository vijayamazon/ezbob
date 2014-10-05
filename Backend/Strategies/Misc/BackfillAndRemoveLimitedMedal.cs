namespace EzBob.Backend.Strategies.Misc 
{
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;
	using LimitedMedalCalculation;

	public class BackfillAndRemoveLimitedMedal : AStrategy
	{
		public BackfillAndRemoveLimitedMedal(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
		}

		public override string Name {
			get { return "BackfillAndRemoveLimitedMedal"; }
		}

		public override void Execute()
		{
			var limitedMedalDualCalculator = new LimitedMedalDualCalculator(DB, Log);
			IEnumerable<SafeReader> readers = DB.ExecuteEnumerable("GetCustomersForMedalBackfill", CommandSpecies.StoredProcedure);

			foreach (var sr in readers)
			{
				int customerId = sr["CustomerId"];
				string typeOfBusiness = sr["TypeOfBusiness"];
				int numOfHmrcMps = sr["NumOfHmrcMps"];
				int numOfEntriesInOfflineScoringTable = sr["NumOfEntriesInOfflineScoringTable"];
				bool isLimited = typeOfBusiness == "LLP" || typeOfBusiness == "Limited";

				if (numOfEntriesInOfflineScoringTable > 0 && (!isLimited || numOfHmrcMps > 1))
				{
					DB.ExecuteNonQuery("RemoveCustomerFromOfflineScoring", CommandSpecies.StoredProcedure, new QueryParameter("CustomerId", customerId));
				}
				else if (numOfEntriesInOfflineScoringTable == 0 && isLimited && numOfHmrcMps < 2)
				{
					limitedMedalDualCalculator.CalculateMedalScore(customerId);
				}
			}
		}
	}
}
