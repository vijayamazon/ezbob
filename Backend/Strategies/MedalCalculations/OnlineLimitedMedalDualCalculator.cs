namespace EzBob.Backend.Strategies.MedalCalculations
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;

	public class OnlineLimitedMedalDualCalculator
	{
		private readonly ASafeLog log;
		private readonly AConnection db;
		private readonly OnlineLimitedMedalCalculator1 onlineLimitedMedalCalculator1;
		//private readonly OnlineLimitedMedalCalculator2 onlineLimitedMedalCalculator2;

		public ScoreResult Results { get; set; }

		public OnlineLimitedMedalDualCalculator(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;
			onlineLimitedMedalCalculator1 = new OnlineLimitedMedalCalculator1(db, log);
			//onlineLimitedMedalCalculator2 = new OnlineLimitedMedalCalculator2(db, log);
		}

		public ScoreResult CalculateMedalScore(int customerId, DateTime calculationTime)
		{
			try
			{
				ScoreResult result1 = onlineLimitedMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
				//ScoreResult result2 = onlineLimitedMedalCalculator2.CalculateMedalScore(customerId, calculationTime);

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
				log.Warn("Online Limited medal calculation for customer {0} failed with exception:{1}", customerId, e);
			}

			return null;
		}
	}
}
