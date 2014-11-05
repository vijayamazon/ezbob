namespace EzBob.Backend.Strategies.MedalCalculations
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;

	public class LimitedMedalDualCalculator
	{
		private readonly ASafeLog log;
		private readonly AConnection db;
		private readonly LimitedMedalCalculator1 limitedMedalCalculator1;
		private readonly LimitedMedalCalculator2 limitedMedalCalculator2;

		public ScoreResult Results { get; set; }

		public LimitedMedalDualCalculator(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;
			limitedMedalCalculator1 = new LimitedMedalCalculator1(db, log);
			limitedMedalCalculator2 = new LimitedMedalCalculator2(db, log);
		}

		public ScoreResult CalculateMedalScore(int customerId, DateTime calculationTime)
		{
			try
			{
				ScoreResult result1 = limitedMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
				ScoreResult result2 = limitedMedalCalculator2.CalculateMedalScore(customerId, calculationTime);

				if (result1.IsIdentical(result2))
				{
					result1.SaveToDb(db);
					return result1;
				}

				log.Error("Mismatch found in the 2 medal calculations of customer: {0}", customerId);
				return null;
			}
			catch (Exception e)
			{
				log.Warn("Limited medal calculation for customer {0} failed with exception:{1}", customerId, e);
			}

			return null;
		}
	}
}
