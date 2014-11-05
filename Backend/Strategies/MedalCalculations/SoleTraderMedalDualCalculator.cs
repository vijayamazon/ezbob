namespace EzBob.Backend.Strategies.MedalCalculations
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using System;

	public class SoleTraderMedalDualCalculator
	{
		private readonly ASafeLog log;
		private readonly AConnection db;
		private readonly SoleTraderMedalCalculator1 soleTraderMedalCalculator1;
		//private readonly SoleTraderMedalCalculator2 soleTraderMedalCalculator2;

		public ScoreResult Results { get; set; }

		public SoleTraderMedalDualCalculator(AConnection db, ASafeLog log)
		{
			this.log = log;
			this.db = db;
			soleTraderMedalCalculator1 = new SoleTraderMedalCalculator1(db, log);
			//soleTraderMedalCalculator2 = new SoleTraderMedalCalculator2(db, log);
		}

		public ScoreResult CalculateMedalScore(int customerId, DateTime calculationTime)
		{
			try
			{
				ScoreResult result1 = soleTraderMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
				//ScoreResult result2 = soleTraderMedalCalculator2.CalculateMedalScore(customerId, calculationTime);

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
				log.Warn("SoleTrader medal calculation for customer {0} failed with exception:{1}", customerId, e);
			}

			return null;
		}
	}
}
