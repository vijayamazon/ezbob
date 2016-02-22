namespace AutomationVerification {
	using System;
	using System.Collections.Generic;
	using AutomationCalculator.AutoDecision.AutoRejection;
	using AutomationCalculator.Common;
	using AutomationCalculator.MedalCalculation;
	using Ezbob.Database;
	using Ezbob.Logger;
	using NUnit.Framework;

	[TestFixture]
	class AutomationTestFixure {
		[SetUp]
		public void Init() {
			_db = DbConnectionGenerator.Get(Log);
		}

		[Test]
		public void TestMedalCalculation() {
			var msc = new OfflineLImitedMedalCalculator(_db, Log);
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
		public void TestMedalChooser() {
			var medalChooser = new MedalChooser(_db, Log);
			var medal = medalChooser.GetMedal(18539, new DateTime(2014, 11, 10));
			Assert.AreEqual(Medal.Gold, medal.Medal);
		}

		[Test]
		public void TestTestMedal() {
			var medalTests = new MedalTests(_db, Log);
			var passed = medalTests.TestMedalCalculation();
			Assert.AreEqual(true, passed);
			//var db = new DbHelper(Log);
			//var models = db.GetMedalTestData();
			//Assert.IsTrue(models.Count > 0);
		}

		[Test]
		public void TestTestDataGatherForMedal() {
			var medalTests = new MedalTests(_db, Log);
			medalTests.TestMedalDataGathering();
		}

		[Test]
		public void TestMedal() {
			var medalChooser = new MedalChooser(_db, Log);
			medalChooser.GetMedal(18626, DateTime.UtcNow);
		}

		[Test]
		public void TestReApprovalData() {
			var db = new DbHelper(_db, Log);
			var model = db.GetAutoReApprovalInputData(14223);
			Assert.AreEqual(900, model.ApprovedAmount);
		}

		[Test]
		public void GetRejectionConfigs() {
			var db = new DbHelper(_db, Log);
			var conf = db.GetRejectionConfigs();
			Assert.GreaterOrEqual(250000, conf.AutoRejectionException_AnualTurnover);
		}

		[Test]
		public void TestConsumerLates() {
			var db = _db;
			var calc = new CaisStatusesCalculation(db, Log);

			//var caisStatuses = new List<CaisStatus> {
			//	new CaisStatus { AccountStatusCodes = "00200", LastUpdatedDate = new DateTime(2014,11,23)}, //0month 1
			//	new CaisStatus { AccountStatusCodes = "00400", LastUpdatedDate = new DateTime(2014,10,23)}, //1month 1
			//	new CaisStatus { AccountStatusCodes = "00020", LastUpdatedDate = new DateTime(2014,10,23)}, //1month 1
			//	new CaisStatus { AccountStatusCodes = "00600", LastUpdatedDate = new DateTime(2014,09,23)}, //2month 0
			//	new CaisStatus { AccountStatusCodes = "00050", LastUpdatedDate = new DateTime(2014,09,23)}, //2month 0
			//	new CaisStatus { AccountStatusCodes = "00005", LastUpdatedDate = new DateTime(2014,09,23)}, //2month 1
			//	new CaisStatus { AccountStatusCodes = "00300", LastUpdatedDate = new DateTime(2014,08,23)}, //3month 0
			//	new CaisStatus { AccountStatusCodes = "00030", LastUpdatedDate = new DateTime(2014,08,23)}, //3month 0
			//	new CaisStatus { AccountStatusCodes = "00003", LastUpdatedDate = new DateTime(2014,08,23)}, //3month 1
			//	new CaisStatus { AccountStatusCodes = "00600", LastUpdatedDate = new DateTime(2014,07,23)}, //4month 0
			//	new CaisStatus { AccountStatusCodes = "00060", LastUpdatedDate = new DateTime(2014,07,23)}, //4month 0
			//	new CaisStatus { AccountStatusCodes = "00006", LastUpdatedDate = new DateTime(2014,07,23)}, //4month 0
			//};
			//var lates = calc.GetLates(1, new DateTime(2014, 11, 23), 1, 3, caisStatuses);

			//Assert.AreEqual(150, lates.LateDays);
			//Assert.AreEqual(5, lates.NumOfLates);

			//lates = calc.GetLates(21370, new DateTime(2014, 11, 23), 1, 3);
			//Assert.AreEqual(60, lates.LateDays);
			//Assert.AreEqual(2, lates.NumOfLates);

			var caisStatuses = new List<CaisStatus> {
				new CaisStatus { AccountStatusCodes = "8", LastUpdatedDate = new DateTime(2014,10,19)}, //0month 1
				new CaisStatus { AccountStatusCodes = "666", LastUpdatedDate = new DateTime(2014,11,02)}, //1month 1
				new CaisStatus { AccountStatusCodes = "666666666666", LastUpdatedDate = new DateTime(2014,10,05)}, //1month 1
				new CaisStatus { AccountStatusCodes = "666666U66665", LastUpdatedDate = new DateTime(2014,10,19)}, //2month 0
				new CaisStatus { AccountStatusCodes = "000000000000", LastUpdatedDate = new DateTime(2014,10,19)}, //2month 0
				new CaisStatus { AccountStatusCodes = "832100", LastUpdatedDate = new DateTime(2014,10,05)}, //2month 1
				new CaisStatus { AccountStatusCodes = "8", LastUpdatedDate = new DateTime(2014,10,05)}, //3month 0
				new CaisStatus { AccountStatusCodes = "832100", LastUpdatedDate = new DateTime(2014,11,02)}, //3month 0
				new CaisStatus { AccountStatusCodes = "832100110000", LastUpdatedDate = new DateTime(2014,10,05)}, //3month 1
			};

			var lates = calc.GetLates(1, new DateTime(2014, 12, 1), 1, 3, caisStatuses);

			Assert.AreEqual(180, lates.LateDays);
			Assert.AreEqual(3, lates.NumOfLates);
		}

		public static readonly ASafeLog Log = new ConsoleLog();
		private AConnection _db;
	}
}
