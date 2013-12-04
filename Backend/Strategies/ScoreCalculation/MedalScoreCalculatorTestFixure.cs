namespace EzBob.Backend.Strategies.ScoreCalculation
{
	using NUnit.Framework;

	[TestFixture]
	class MedalScoreCalculatorTestFixture
	{
		[Test]
		public void testMedalCalculation()
		{
			MedalScoreCalculator msc = new MedalScoreCalculator();
			var medal = msc.CalculateMedalScore(125000, 740, 8, 10000, MaritalStatus.Married, Gender.M, 0, false, 1.2M, 0, 0, 0);

			Assert.AreEqual(medal.Medal, Medal.Silver);
		}

		[Test]
		public void testMedalCalculation2()
		{
			MedalScoreCalculator msc = new MedalScoreCalculator();
			var medal = msc.CalculateMedalScore(125000, 900, 8, 10000, MaritalStatus.Married, Gender.M, 3, false, 0, 0, 0, 0);

			Assert.AreEqual(medal.Medal, Medal.Gold);
		}
	}
}
