namespace EzBob.Backend.Strategies.ScoreCalculation
{
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Logger;
	using NUnit.Framework;

	[TestFixture]
	class MedalScoreCalculatorTestFixture
	{
		[Test]
		public void testMedalCalculation()
		{
			MedalScoreCalculator msc = new MedalScoreCalculator(new SafeLog());
			var medal = msc.CalculateMedalScore(125000, 740, 8, 10000, MaritalStatus.Married, Gender.M, 0, false, 1.2M, 0, 0, 0);

			Assert.AreEqual(medal.Medal, MedalMultiplier.Silver);
		}

		[Test]
		public void testMedalCalculation2()
		{
			MedalScoreCalculator msc = new MedalScoreCalculator(new SafeLog());
			var medal = msc.CalculateMedalScore(125000, 900, 8, 10000, MaritalStatus.Married, Gender.M, 3, false, 0, 0, 0, 0);

			Assert.AreEqual(medal.Medal, MedalMultiplier.Gold);
		}
	}
}
