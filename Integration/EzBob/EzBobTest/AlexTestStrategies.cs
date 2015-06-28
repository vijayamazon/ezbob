namespace EzBobTest {
	using System;
	using Ezbob.Backend.Strategies.AutomationVerification.CharlieAutomationReport;
	using NUnit.Framework;

	[TestFixture]
	public class AlexTestStrategies : BaseTestFixtue {
		[Test]
		public void TestCaisAccountIsBad() {
			bool res = CarCaisAccount.IsBad(
				new DateTime(2015, 6, 4, 7, 34, 56, DateTimeKind.Utc),
				new DateTime(2015, 5, 3, 0, 0, 0, DateTimeKind.Utc),
				1000,
				"000000000000"
			);

			m_oLog.Debug("Result is: {0}", res ? "bad" : "good");
		} // TestCaisAccountIsBad
	} // class AlexTestStrategies
} // namespace