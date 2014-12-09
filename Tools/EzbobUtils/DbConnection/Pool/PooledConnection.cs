namespace Ezbob.Database.Pool {
	using System.Data.Common;
	using Ezbob.Utils.ObjectPool;

	public class PooledConnection : IPoolable {

		public PooledConnection() {
			IsPooled = true;
			OutOfPoolCount = 0;
		} // constructor

		public ulong PoolItemID {
			get { return m_nPoolItemID; }
			set {
				m_nPoolItemID = value;
				SetName();
			} // set
		} // PoolItemID

		private ulong m_nPoolItemID;

		public bool IsPooled {
			get { return m_bIsPooled; }
			set {
				m_bIsPooled = value;
				SetName();
			} // set
		} // IsPooled

		private bool m_bIsPooled;

		public string Name {
			get { return m_sName; }
		} // Name

		private void SetName() {
			m_sName = string.Format("{1}-{0}", PoolItemID, IsPooled ? "pool" : "pool-free");
		} // SetName

		private string m_sName;

		public DbConnection Connection { get; set; }
		public uint OutOfPoolCount { get; set; }
	} // class PooledConnection
} // namespace
