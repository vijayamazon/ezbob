namespace AutomationVerification
{
	using System;
	using AutomationCalculator.AutoDecision;
	using AutomationCalculator.Common;
	using AutomationCalculator.MedalCalculation;
	using Ezbob.Logger;
	using NUnit.Framework;

	[TestFixture]
	class AutomationTestFixure
	{
		public static readonly ASafeLog Log = new ConsoleLog();

		[Test]
		public void TestMedalCalculation()
		{
			var msc = new OfflineLImitedMedalCalculator(Log);
			var data = new MedalInputModel
			{
				AnnualTurnover = 125000,
				BusinessScore = 55,
				MaritalStatus = MaritalStatus.Married,
				TangibleEquity = 0.1M,
				BusinessSeniority = 2,
				ConsumerScore = 850,
				EzbobSeniority = 7,
				FirstRepaymentDatePassed = false,
				FreeCashFlow = 0.42M,
				HasHmrc = true,
				NetWorth = 0.3M,
				NumOfEarlyPayments = 0,
				NumOfLatePayments = 0,
				NumOfOnTimeLoans = 0

			};
			var medal = msc.CalculateMedal(data);

			Assert.AreEqual(Medal.Gold, medal.Medal);
		}

		[Test]
		public void TestMedalCalculation2()
		{
			var msc = new OfflineLImitedMedalCalculator(Log);
			var data = msc.GetInputParameters(14223);
			var medal = msc.CalculateMedal(data);

			Assert.AreEqual(medal.Medal, Medal.Gold);
		}

		[Test]
		public void TestAutoRerejetion()
		{
			var arr = new AutoReRejectionCalculator(Log);
			string reason;
			var des = arr.IsAutoReRejected(10144, 150232, out reason);
			Assert.AreEqual(true, des);
		}

		[Test]
		public void TestAutoRejetion()
		{
			var db = new DbHelper(Log);
			var rejectionConstants = db.GetRejectionConstants();
			var arr = new AutoRejectionCalculator(Log, rejectionConstants);
			string reason;
			var des = arr.IsAutoRejected(5894, out reason);
			Assert.AreEqual(false, des);
		}

		[Test]
		public void TestAutoReapproval()
		{
			var arr = new AutoReApprovalCalculator(Log);
			string reason;
			int amount;
			var des = arr.IsAutoReApproved(14223, 0, out reason, out amount);
			Assert.AreEqual(false, des);
		}

		[Test]
		public void TestAutoApproval()
		{
			var arr = new AutoApprovalCalculator(Log);
			string reason;
			int amount;
			var des = arr.IsAutoApproved(14223, out reason, out amount);
			Assert.AreEqual(false, des);
		}

		[Test]
		public void TestIsOffline()
		{
			var db = new DbHelper(Log);
			var isOffline = db.IsOffline(25);
			Assert.AreEqual(false, isOffline);

			isOffline = db.IsOffline(14183);
			Assert.AreEqual(true, isOffline);
		}

		[Test]
		public void TestJoin()
		{
			var db = new DbHelper(Log);
			var mps = db.GetCustomerPaymentMarketPlaces(16241);
			var s = string.Join(",", mps);
			Assert.IsNotNullOrEmpty(s);
		}

		[Test]
		public void TestFeedbacks()
		{
			var mpHelper = new MarketPlacesHelper(Log);
			var feedbacks = mpHelper.GetPositiveFeedbacks(14166);
			Assert.AreEqual(33885, feedbacks);

		}

		[Test]
		public void TestOnlineTurnover()
		{
			var mpHelper = new MarketPlacesHelper(Log);
			var turnover = mpHelper.GetOnlineTurnoverAnnualized(14166);
			Assert.IsTrue(Math.Abs(turnover - 10812) < 1);
		}

		[Test]
		public void TestMedalChooser()
		{
			var medalChooser = new MedalChooser(Log);
			var medal = medalChooser.GetMedal(14223);
			Assert.AreEqual(Medal.Gold, medal.Medal);
		}


		[Test]
		public void TestTestMedal() {
			var medalTests = new MedalTests(Log);
			var passed = medalTests.TestMedalCalculation();
			Assert.AreEqual(true, passed);
			//var db = new DbHelper(Log);
			//var models = db.GetMedalTestData();
			//Assert.IsTrue(models.Count > 0);
		}

	}
}
