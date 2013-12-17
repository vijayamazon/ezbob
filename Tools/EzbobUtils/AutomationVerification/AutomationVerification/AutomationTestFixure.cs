namespace AutomationVerification
{
	using AutomationCalculator;
	using NUnit.Framework;

	[TestFixture]
	class AutomationTestFixure
	{
		[Test]
		public void testMedalCalculation()
		{
			var msc = new MedalScoreCalculator();
			var medal = msc.CalculateMedalScore(125000, 740, 8, 10000, MaritalStatus.Married, Gender.M, 0, false, 1.2M, 0, 0, 0);

			Assert.AreEqual(medal.Medal, Medal.Silver);
		}

		[Test]
		public void testMedalCalculation2()
		{
			var msc = new MedalScoreCalculator();
			var medal = msc.CalculateMedalScore(125000, 900, 8, 10000, MaritalStatus.Married, Gender.M, 3, false, 0, 0, 0, 0);

			Assert.AreEqual(medal.Medal, Medal.Gold);
		}

		[Test]
		public void testAutoRerejetion()
		{
			var arr = new AutoReRejectionCalculator();
			string reason;
			var des = arr.IsAutoReRejected(14223, out reason);
			Assert.AreEqual(des, false);
		}
	}
}
