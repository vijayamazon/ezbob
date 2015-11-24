namespace EzBobTest {
	using System;
	using NUnit.Framework;
	using Reports.EarnedInterest;

	[TestFixture]
	class TestEarnedInterest : BaseTestFixtue {
		[Test]
		public void TestLifeTime() {
			var ei = new EarnedInterest(
				m_oDB,
				EarnedInterest.WorkingMode.ForPeriod,
				false,
				new DateTime(2012, 9, 1, 0, 0, 0, DateTimeKind.Utc),
				DateTime.UtcNow,
				m_oLog
			);

			ei.VerboseLogging = true;

			ei.Run();
		} // TestLifeTime

		[Test]
		public void TestOneDay() {
			var ei = new EarnedInterest(
				m_oDB,
				EarnedInterest.WorkingMode.ForPeriod,
				false,
				new DateTime(2014, 8, 23, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2014, 8, 24, 0, 0, 0, DateTimeKind.Utc),
				m_oLog
			);

			ei.VerboseLogging = true;

			ei.Run();
		} // TestOneDay
	} // class TestEarnedInterest
} // namespace
