namespace AutomationVerification
{
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.AutoDecision;
	using AutomationCalculator.AutoDecision.AutoRejection;
	using AutomationCalculator.Common;
	using AutomationCalculator.MedalCalculation;
	using AutomationCalculator.OfferCalculation;
	using Ezbob.Database;
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
			var rejectionConstants = db.GetRejectionConfigs();
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
			var des = arr.IsAutoReApproved(14223, 0, out reason, out amount);
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
		public void TestTestMedal()
		{
			var medalTests = new MedalTests(Log);
			var passed = medalTests.TestMedalCalculation();
			Assert.AreEqual(true, passed);
			//var db = new DbHelper(Log);
			//var models = db.GetMedalTestData();
			//Assert.IsTrue(models.Count > 0);
		}

		[Test]
		public void TestTestDataGatherForMedal()
		{
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
			var input = new OfferInputModel {
				Amount = 17000,
				HasLoans = false,
				AspireToMinSetupFee = true,
				Medal = Medal.Gold,
				CustomerId = 18263,
			};

			var offer1 = offerCalculator.GetOfferBySeek(input);
			var offer2 = offerCalculator.GetOfferByBoundaries(input);

			Log.Debug("Offer1 is: {0}", offer1);
			Log.Debug("Offer2 is: {0}", offer2);
			Assert.AreEqual(offer1.InterestRate, offer2.InterestRate);
			Assert.AreEqual(offer1.SetupFee, offer2.SetupFee);
			var db = new SqlConnection(Log);
			offer1.SaveToDb(Log, db, OfferCalculationType.Seek);
			offer2.SaveToDb(Log, db, OfferCalculationType.Boundaries);
		}

		[Test]
		public void TestOriginationTime()
		{
			var db = new DbHelper(Log);
			var origTime = db.GetCustomerMarketPlacesOriginationTimes(14223);
			Assert.NotNull(origTime.Since);
		}

		[Test]
		public void TestReApprovalData()
		{
			var db = new DbHelper(Log);
			var model = db.GetAutoReApprovalInputData(14223);
			Assert.AreEqual(900, model.ApprovedAmount);
		}

		[Test]
		public void TestReApprovalAgent()
		{
			var agent = new AutomationCalculator.AutoDecision.AutoReApproval.Agent(Log, 14223);
			var data = agent.GetInputData();
			agent.MakeDecision(data);
			Assert.AreEqual(false, agent.Result.IsAutoReApproved);
		}

		[Test]
		public void GetRejectionConfigs()
		{
			var db = new DbHelper(Log);
			var conf = db.GetRejectionConfigs();
			Assert.GreaterOrEqual(250000, conf.AutoRejectionException_AnualTurnover);
		}

		[Test]
		public void TestConsumerLates() {
			var calc = new ConsumerLatesCalculation(Log);

			var caisStatuses = new List<CaisStatus> {
				new CaisStatus { AccountStatusCodes = "00200", LastUpdatedDate = new DateTime(2014,11,23)}, //0month 1
				new CaisStatus { AccountStatusCodes = "00400", LastUpdatedDate = new DateTime(2014,10,23)}, //1month 1
				new CaisStatus { AccountStatusCodes = "00020", LastUpdatedDate = new DateTime(2014,10,23)}, //1month 1
				new CaisStatus { AccountStatusCodes = "00600", LastUpdatedDate = new DateTime(2014,09,23)}, //2month 0
				new CaisStatus { AccountStatusCodes = "00050", LastUpdatedDate = new DateTime(2014,09,23)}, //2month 0
				new CaisStatus { AccountStatusCodes = "00005", LastUpdatedDate = new DateTime(2014,09,23)}, //2month 1
				new CaisStatus { AccountStatusCodes = "00300", LastUpdatedDate = new DateTime(2014,08,23)}, //3month 0
				new CaisStatus { AccountStatusCodes = "00030", LastUpdatedDate = new DateTime(2014,08,23)}, //3month 0
				new CaisStatus { AccountStatusCodes = "00003", LastUpdatedDate = new DateTime(2014,08,23)}, //3month 1
				new CaisStatus { AccountStatusCodes = "00600", LastUpdatedDate = new DateTime(2014,07,23)}, //4month 0
				new CaisStatus { AccountStatusCodes = "00060", LastUpdatedDate = new DateTime(2014,07,23)}, //4month 0
				new CaisStatus { AccountStatusCodes = "00006", LastUpdatedDate = new DateTime(2014,07,23)}, //4month 0
			};
			var lates = calc.GetLates(1, new DateTime(2014, 11, 23), 1, 3, caisStatuses);
			
			Assert.AreEqual(150, lates.LateDays);
			Assert.AreEqual(5, lates.NumOfLates);

			lates = calc.GetLates(21370, new DateTime(2014, 11, 23), 1, 3);
			Assert.AreEqual(60, lates.LateDays);
			Assert.AreEqual(2, lates.NumOfLates);

		}

		[Test]
		public void TestRejectionTurnover() {
			int[] customerIds = new int[]{25,26,27,28,29,30,14210,14214,14215,14216,14217,14218,14219,14220,14221,14222,14223,14226,14227,14228,14229,15227,15228,15230,15231,15232,16232,16236,16237,16238,16240,16241,16242,16243,16244,16245,16246,16247,16248,16249,16250,17250,17251,17252,17253,17254,17256,17258,17259,17260,17261,17262,17263,17264,17265,17266,17267,18268,18269,18270,18271,18273,18274,18275,18285,18286,18287,18289,18290,20290,20291,20292,20302,20304,20319,20321,21322,21323,21327,21333,21335,21336,21337,21338,21340,21341,21342,21343,21344,21345,21364,21370,21371,21372,21377,21378,21387,21388,21389,21390,21394,21399,21400,21402,21403,21404};
			var mpHelper = new MarketPlacesHelper(Log);
			foreach (var customerId in customerIds) {
				mpHelper.GetTurnoverForRejection(customerId);
			}
		}

		[Test]
		public void TestAutoRejection()
		{
			int[] customerIds = new int[] { 25, 26, 27, 28, 29, 30, 14210, 14214, 14215, 14216, 14217, 14218, 14219, 14220, 14221, 14222, 14223, 14226, 14227, 14228, 14229, 15227, 15228, 15230, 15231, 15232, 16232, 16236, 16237, 16238, 16240, 16241, 16242, 16243, 16244, 16245, 16246, 16247, 16248, 16249, 16250, 17250, 17251, 17252, 17253, 17254, 17256, 17258, 17259, 17260, 17261, 17262, 17263, 17264, 17265, 17266, 17267, 18268, 18269, 18270, 18271, 18273, 18274, 18275, 18285, 18286, 18287, 18289, 18290, 20290, 20291, 20292, 20302, 20304, 20319, 20321, 21322, 21323, 21327, 21333, 21335, 21336, 21337, 21338, 21340, 21341, 21342, 21343, 21344, 21345, 21364, 21370, 21371, 21372, 21377, 21378, 21387, 21388, 21389, 21390, 21394, 21399, 21400, 21402, 21403, 21404 };
			var dbHelper = new DbHelper(Log);
			var rejectionConfigs = dbHelper.GetRejectionConfigs();
			var db = new SqlConnection(Log);
			int autoRejected = 0;
			int notAutoRejected = 0;
			foreach (var customerId in customerIds) {
				var rejectionAgent = new RejectionAgent(db, Log, customerId, rejectionConfigs);
				rejectionAgent.MakeDecision(rejectionAgent.GetRejectionInputData(null));
				autoRejected = rejectionAgent.IsAutoRejected ? autoRejected + 1 : autoRejected;
				notAutoRejected = rejectionAgent.IsAutoRejected ? notAutoRejected : notAutoRejected + 1;
			}

			Log.Debug("Run on {0} rejected {1} not rejected {2}", customerIds.Length, autoRejected, notAutoRejected);

			Assert.Greater(autoRejected, 0);
			Assert.Greater(notAutoRejected, 0);
		}
	}
}
