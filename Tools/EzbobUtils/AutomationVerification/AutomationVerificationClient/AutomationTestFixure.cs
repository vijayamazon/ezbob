namespace AutomationVerification
{
	using System;
	using AutomationCalculator.AutoDecision;
	using AutomationCalculator.Common;
	using AutomationCalculator.MedalCalculation;
	using AutomationCalculator.OfferCalculation;
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
			var data = new MedalInputModel {
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
			var des = arr.IsAutoReRejected(10144,150232, out reason);
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
			int amount = 0;
			var des = arr.IsAutoReApproved(14223,0, out reason, out amount);
			Assert.AreEqual(false, des);
		}

		[Test]
		public void TestAutoApproval()
		{
			var arr = new AutoApprovalCalculator(Log);
			string reason;
			int amount = 0;
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
			//var turnover = mpHelper.GetOnlineTurnoverAnnualized(15689);
			//Assert.IsTrue(Math.Abs(turnover - 45299.52M) < 1);

			//turnover = mpHelper.GetOnlineTurnoverAnnualized(15898);
			//Assert.IsTrue(Math.Abs(turnover - 125762.575M) < 1);

			//turnover = mpHelper.GetOnlineTurnoverAnnualized(16305);
			//Assert.IsTrue(Math.Abs(turnover - 107665.91M) < 1);

			//turnover = mpHelper.GetOnlineTurnoverAnnualized(16387);
			//Assert.IsTrue(Math.Abs(turnover - 140229.9M) < 1);

			//turnover = mpHelper.GetOnlineTurnoverAnnualized(16448);
			//Assert.IsTrue(Math.Abs(turnover - 390054.541935M) < 1);

			var turnover = mpHelper.GetOnlineTurnoverAnnualized(15885);
			Assert.IsTrue(Math.Abs(turnover - 11826.403198M) < 1);

			turnover = mpHelper.GetOnlineTurnoverAnnualized(16254);
			Assert.IsTrue(Math.Abs(turnover - 98838.020323M) < 1);

			turnover = mpHelper.GetOnlineTurnoverAnnualized(16620);
			Assert.IsTrue(Math.Abs(turnover - 42177.345988M) < 1);

			turnover = mpHelper.GetOnlineTurnoverAnnualized(16717);
			Assert.IsTrue(Math.Abs(turnover - 625.526154M) < 1);

			turnover = mpHelper.GetOnlineTurnoverAnnualized(17322);
			Assert.IsTrue(Math.Abs(turnover - 261.676923M) < 1);

		}

		[Test]
		public void TestMedalChooser()
		{
			var medalChooser = new MedalChooser(Log);
			var medal = medalChooser.GetMedal(18539, new DateTime(2014, 11, 10));
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

		[Test]
		public void TestTestDataGatherForMedal() {
			var medalTests = new MedalTests(Log);
			medalTests.TestMedalDataGathering();
		}

		[Test]
		public void TestMedal()
		{
			var medalChooser = new MedalChooser(Log);
			medalChooser.GetMedal(18626);
		}

		[Test]
		public void TestOfferCalculator()
		{
			var offerCalculator = new OfferCalculator(Log);
			var offer = offerCalculator.GetOffer(new OfferInputModel() {
				Amount = 1000,
				HasLoans = false,
				AspireToMinSetupFee = true,
				Medal = Medal.Gold
			});

			Assert.GreaterOrEqual(offer.InterestRate, 0.03M);
		}

		[Test]
		public void TestOriginationTime() {
			var db = new DbHelper(Log);
			var origTime = db.GetCustomerMarketPlacesOriginationTimes(14223);
			Assert.NotNull(origTime.Since);
		}

		[Test]
		public void TestReApprovalData() {
			var db = new DbHelper(Log);
			var model = db.GetAutoReApprovalInputData(14223);
			Assert.AreEqual(900, model.ApprovedAmount);
		}
	}
}
