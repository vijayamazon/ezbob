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
			var msc = new MedalScoreCalculator(new ConsoleLog());
			var medal = msc.CalculateMedalScore(125000, 740, 8, 10000, MaritalStatus.Married, Gender.M, 0, false, 1.2M, 0, 0, 0);
			Assert.AreEqual(medal.Medal, MedalMultiplier.Silver);
			
			medal = msc.CalculateMedalScore(176255, 1201, 6.5M, 1072, MaritalStatus.Married, Gender.M, 3, true, 15, 2, 0, 0);
			Assert.AreEqual(medal.Medal, MedalMultiplier.Platinum);

			medal = msc.CalculateMedalScore(72214, 951, 6.5M, 1552, MaritalStatus.Married, Gender.M, 3, true, 16.6M, 7, 0, 5);
			Assert.AreEqual(medal.Medal, MedalMultiplier.Platinum);

			medal = msc.CalculateMedalScore(30678, 939, 2.58M, 100, MaritalStatus.Married, Gender.M, 2, true, 16.4M, 2, 0, 4);
			Assert.AreEqual(medal.Medal, MedalMultiplier.Gold);
			//61.5% 2500pound at 5%

			medal = msc.CalculateMedalScore(108916, 1092, 6.25M, 7250, MaritalStatus.Married, Gender.M, 1, true, 15.2M, 5, 0, 5);
			Assert.AreEqual(medal.Medal, MedalMultiplier.Platinum);
			//79.3% 10900pounds at 4%

			medal = msc.CalculateMedalScore(555912, 982, 7.08M, 5000, MaritalStatus.Married, Gender.M, 3, true, 14.2M, 1, 0, 0);
			Assert.AreEqual(medal.Medal, MedalMultiplier.Gold);
			//53.9% 44500pounds at 5%

			//annualTurnover: 156436.91, experianScore: 946, mpSeniorityYears: 92.64657534246575342465753425, positiveFeedbackCount: 30990, maritalStatus: Married, gender: M, numberOfStores(eBay\Amazon\PayPal): 3, firstRepaymentDatePassed: True, ezbobSeniorityMonths: 264, ezbobNumOfLoans: 0, ezbobNumOfLateRepayments: 0, ezbobNumOfEarlyReayments: 0
			medal = msc.CalculateMedalScore(156436.91M, 946, 92.64657534246575342465753425M, 30990, MaritalStatus.Married, Gender.M, 3, true, 264M, 0, 0, 0);
			Assert.AreEqual(medal.Medal, MedalMultiplier.Gold);


			
		}

		[Test]
		public void testMedalCalculation2()
		{
			var msc = new MedalScoreCalculator(new ConsoleLog());
			var medal = msc.CalculateMedalScore(125000, 900, 8, 10000, MaritalStatus.Married, Gender.M, 3, false, 0, 0, 0, 0);

			Assert.AreEqual(medal.Medal, MedalMultiplier.Gold);
		}

		[Test]
		public void testPrcent()
		{
			var msc = new MedalScoreCalculator(new ConsoleLog());
			var offer = msc.GetRange(Constants.OfferPercentRanges, 0).OfferPercent;
			Assert.AreEqual(0.07M, offer);

			offer = msc.GetRange(Constants.OfferPercentRanges, 720).OfferPercent;
			Assert.AreEqual(0.06M, offer);

			offer = msc.GetRange(Constants.OfferPercentRanges, 900).OfferPercent;
			Assert.AreEqual(0.05M, offer);

			offer = msc.GetRange(Constants.OfferPercentRanges, 1020).OfferPercent;
			Assert.AreEqual(0.04M, offer);

			offer = msc.GetRange(Constants.OfferPercentRanges, 1220).OfferPercent;
			Assert.AreEqual(0.03M, offer);
		}
	}
}
