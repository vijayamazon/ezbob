namespace EzBobTest {
	using NUnit.Framework;

	[TestFixture]
	public class AlexTestStrategies : BaseTestFixtue {
		[Test]
		public void TestCaisAccountIsBad() {
			m_oLog.Debug("Result is: {0}", true ? "bad" : "good");
		} // TestCaisAccountIsBad
	} // class AlexTestStrategies
} // namespace