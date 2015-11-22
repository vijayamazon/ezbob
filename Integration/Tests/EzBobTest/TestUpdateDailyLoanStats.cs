namespace EzBobTest {
	using System;
	using Ezbob.Backend.Strategies.Tasks;
	using NUnit.Framework;

	[TestFixture]
	class TestUpdateDailyLoanStats : BaseTestFixtue {
		[Test]
		public void Test() {
			var dueis = new UpdateDailyLoanStats(10, new DateTime(2014, 7, 1, 0, 0, 0, DateTimeKind.Utc));
			dueis.Execute();
		} // Test
	} // class TestDailyUpdateEarnedInterestStats
} // namespace
