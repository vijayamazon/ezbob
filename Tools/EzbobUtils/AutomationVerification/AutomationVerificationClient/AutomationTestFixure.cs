namespace AutomationVerification
{
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
		public void testMedalCalculation()
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
		public void testMedalCalculation2()
		{
			var msc = new OfflineLImitedMedalCalculator(Log);
			var data = msc.GetInputParameters(14223);
			var medal = msc.CalculateMedal(data);

			Assert.AreEqual(medal.Medal, Medal.Gold);
		}

		[Test]
		public void testAutoRerejetion()
		{
			var arr = new AutoReRejectionCalculator(Log);
			string reason;
			var des = arr.IsAutoReRejected(10144,150232, out reason);
			Assert.AreEqual(true, des);
		}

		[Test]
		public void testAutoRejetion()
		{
			var db = new DbHelper(Log);
			var rejectionConstants = db.GetRejectionConstants();
			var arr = new AutoRejectionCalculator(Log, rejectionConstants);
			string reason;
			var des = arr.IsAutoRejected(5894, out reason);
			Assert.AreEqual(false, des);
		}

		[Test]
		public void testAutoReapproval()
		{
			var arr = new AutoReApprovalCalculator(Log);
			string reason;
			int amount = 0;
			var des = arr.IsAutoReApproved(14223,0, out reason, out amount);
			Assert.AreEqual(false, des);
		}

		[Test]
		public void testAutoApproval()
		{
			var arr = new AutoApprovalCalculator(Log);
			string reason;
			int amount = 0;
			var des = arr.IsAutoApproved(14223, out reason, out amount);
			Assert.AreEqual(false, des);
		}

		[Test]
		public void testIsOffline()
		{
			var db = new DbHelper(Log);
			var isOffline = db.IsOffline(25);
			Assert.AreEqual(false, isOffline);

			isOffline = db.IsOffline(14183);
			Assert.AreEqual(true, isOffline);
		}

		[Test]
		public void testJoin()
		{
			var db = new DbHelper(Log);
			var mps = db.GetCustomerPaymentMarketPlaces(16241);
			var s = string.Join(",", mps);
			Assert.IsNotNullOrEmpty(s);
		}
	}
}
