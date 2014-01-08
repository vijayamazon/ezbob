namespace AutomationVerification
{
	using AutomationCalculator;
	using CommonLib;
	using Ezbob.Logger;
	using NUnit.Framework;

	[TestFixture]
	class AutomationTestFixure
	{
		public static readonly ASafeLog Log = new ConsoleLog();

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
			var arr = new AutoReRejectionCalculator(Log);
			string reason;
			var des = arr.IsAutoReRejected(7822, out reason);
			Assert.AreEqual(false, des);
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
			var des = arr.IsAutoReApproved(14223, out reason, out amount);
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
	}
}
