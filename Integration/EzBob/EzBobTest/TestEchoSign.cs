namespace EzBobTest {
	using EchoSignLib;
	using NUnit.Framework;

	[TestFixture]
	class TestEchoSign : BaseTestFixtue {
		[Test]
		public void TestConfiguration() {
			var esf = new EchoSignFacade(m_oDB, m_oLog);
		} // TestConfiguration
	} // class TestEchoSign
} // namespace
