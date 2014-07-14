namespace EzBobTest {
	using System.Collections.Generic;
	using Ezbob.Database;
	using NUnit.Framework;

	[TestFixture]
	class TestDbConnection : BaseTestFixtue {
		[Test]
		public void TestConfiguration() {
			var cw = m_oDB.GetPersistent();

			m_oLog.Debug("First customer ID: {0}", m_oDB.ExecuteScalar<int>(cw, "SELECT TOP 1 Id FROM Customer"));

			m_oLog.Debug("First broker ID: {0}", m_oDB.ExecuteScalar<int>(cw, "SELECT TOP 1 BrokerID FROM Broker"));

			cw.Close();
		} // TestConfiguration

		[Test]
		public void TestEnumerable() {
			IEnumerable<SafeReader> lst = m_oDB.ExecuteEnumerable("SELECT TOP 2 Id, Name FROM Customer", CommandSpecies.Text);

			m_oLog.Debug("Iteration starts...");

			foreach (SafeReader sr in lst)
				m_oLog.Debug("Customer {0}: {1}", (int)sr["Id"], (string)sr["Name"]);

			m_oLog.Debug("Iteration complete.");

			m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					m_oLog.Debug("Broker {0}: {1}", (int)sr[0], (string)sr[1]);
					return ActionResult.Continue;
				},
				"SELECT TOP 2 BrokerID, ContactEmail FROM Broker",
				CommandSpecies.Text
			);

			m_oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					m_oLog.Debug("User {0}: {1}", (int)sr[0], (string)sr[1]);
					return ActionResult.Continue;
				},
				"SELECT TOP 2 UserId, FullName FROM Security_User",
				CommandSpecies.Text
			);
		} // TestEnumerable

		[Test]
		public void TestEnumerableWithPersistent() {
			var cw = m_oDB.GetPersistent();

			IEnumerable<SafeReader> lst = m_oDB.ExecuteEnumerable(cw, "SELECT TOP 2 Id, Name FROM Customer", CommandSpecies.Text);

			m_oLog.Debug("Iteration starts...");

			foreach (SafeReader sr in lst)
				m_oLog.Debug("Customer {0}: {1}", (int)sr["Id"], (string)sr["Name"]);

			m_oLog.Debug("Iteration complete.");

			m_oDB.ForEachRowSafe(
				cw,
				(sr, bRowsetStart) => {
					m_oLog.Debug("Broker {0}: {1}", (int)sr[0], (string)sr[1]);
					return ActionResult.Continue;
				},
				"SELECT TOP 2 BrokerID, ContactEmail FROM Broker",
				CommandSpecies.Text
			);

			m_oDB.ForEachRowSafe(
				cw,
				(sr, bRowsetStart) => {
					m_oLog.Debug("User {0}: {1}", (int)sr[0], (string)sr[1]);
					return ActionResult.Continue;
				},
				"SELECT TOP 2 UserId, FullName FROM Security_User",
				CommandSpecies.Text
			);

			cw.Close();
		} // TestEnumerableWithPersistent
	} // class TestDbConnection
} // namespace
