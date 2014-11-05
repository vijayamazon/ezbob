namespace EzBob.Backend.Strategies.MedalCalculations
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;

	public class NonLimitedMedalDualCalculator
	{
		private readonly ASafeLog log;
		private readonly AConnection db;
		private readonly NonLimitedMedalCalculator1 nonLimitedMedalCalculator1;
		//private readonly NonLimitedMedalCalculator2 nonLimitedMedalCalculator2;

		public ScoreResult Results { get; set; }

		public NonLimitedMedalDualCalculator(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;
			nonLimitedMedalCalculator1 = new NonLimitedMedalCalculator1(db, log);
			//nonLimitedMedalCalculator2 = new NonLimitedMedalCalculator2(db, log);
		}

		public ScoreResult CalculateMedalScore(int customerId, DateTime calculationTime)
		{
			try
			{
				ScoreResult result1 = nonLimitedMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
				//ScoreResult result2 = nonLimitedMedalCalculator2.CalculateMedalScore(customerId, calculationTime);

				//if (result1.IsIdentical(result2))
				{
					result1.SaveToDb(db);
					return result1;
				}

				//log.Error("Mismatch found in the 2 medal calculations of customer: {0}", customerId);
				//return null;
			}
			catch (Exception e)
			{
				log.Warn("NonLimited medal calculation for customer {0} failed with exception:{1}", customerId, e);
			}

			return null;
		}
	}
}
