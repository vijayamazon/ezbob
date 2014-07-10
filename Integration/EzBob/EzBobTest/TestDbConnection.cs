namespace EzBobTest {
	using NUnit.Framework;

	[TestFixture]
	class TestDbConnection : BaseTestFixtue {
		[Test]
		public void TestConfiguration() {
			m_oDB.OpenPersistent();

			m_oLog.Debug("First customer ID: {0}", m_oDB.ExecuteScalar<int>("SELECT TOP 1 Id FROM Customer"));

			m_oLog.Debug("First broker ID: {0}", m_oDB.ExecuteScalar<int>("SELECT TOP 1 BrokerID FROM Broker"));

			m_oDB.ClosePersistent();
		} // TestConfiguration
	} // class TestDbConnection
} // namespace
