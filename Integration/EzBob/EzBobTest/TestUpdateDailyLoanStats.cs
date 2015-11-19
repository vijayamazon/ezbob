namespace EzBobTest {
	using System;
	using Ezbob.Backend.Strategies.Backfill;
	using Ezbob.Backend.Strategies.Tasks;
	using NUnit.Framework;

	[TestFixture]
	class TestUpdateDailyLoanStats : BaseTestFixtue {
		[Test]
		public void Test() {
			var dueis = new UpdateDailyLoanStats(0, new DateTime(2014, 7, 1, 0, 0, 0, DateTimeKind.Utc));
			dueis.Execute();
		} // Test

		[Test]
		public void TestBackfill() {
			var dueis = new BackfillDailyLoanStats();
			dueis.Execute();
		} // TestBackfill
	} // class TestDailyUpdateEarnedInterestStats
} // namespace
