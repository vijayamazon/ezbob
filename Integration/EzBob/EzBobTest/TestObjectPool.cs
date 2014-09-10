namespace EzBobTest {
	using System;
	using Ezbob.Utils.ObjectPool;
	using NUnit.Framework;

	internal class Poolee : IPoolable {
		public static implicit operator string(Poolee p) {
			return p == null ? "-- null --" : p.ToString();
		} // operator string

		public Poolee() {
			m_oData = Guid.NewGuid();
		} // constructor

		public override string ToString() {
			return m_oData.ToString("N");
		} // ToString

		public ulong PoolItemID { get; set; }

		private Guid m_oData;
	} // class Poolee

	[TestFixture]
	public class TestObjectPool : BaseTestFixtue {
		[Test]
		public void BasicTest() {
			var pool = new ObjectPool<Poolee>(3, m_oLog);

			m_oLog.Debug("Give loop...");

			for (int i = 0; i < 5; i++)
				m_oLog.Debug("From pool: {0}", (string)pool.Give());

			m_oLog.Debug("To pool (invalid object): {0}", pool.Take(null));

			m_oLog.Debug("Take loop...");

			for (int i = 0; i < 5; i++) {
				var p = new Poolee();
				m_oLog.Debug("Sending {0}...", p);
				m_oLog.Debug("To pool (valid object): {0}", pool.Take(p));
			} // for

			m_oLog.Debug("Give loop...");

			for (int i = 0; i < 5; i++)
				m_oLog.Debug("From pool: {0}", (string)pool.Give());

			m_oLog.Debug("Take and give loop...");

			for (int i = 0; i < 5; i++) {
				var p = new Poolee();
				m_oLog.Debug("Sending {0}...", p);
				m_oLog.Debug("To pool (valid object): {0}", pool.Take(p));
				m_oLog.Debug("From pool: {0}", (string)pool.Give());
			} // for
		} // BasicTest
	} // class TestObjectPool
} // namespace
