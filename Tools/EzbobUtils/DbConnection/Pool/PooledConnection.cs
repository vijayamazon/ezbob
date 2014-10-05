namespace Ezbob.Database.Pool {
	using System.Data.Common;
	using Ezbob.Utils.ObjectPool;

	public class PooledConnection : IPoolable {
		#region constructor

		public PooledConnection() {
			IsPooled = true;
			OutOfPoolCount = 0;
		} // constructor

		#endregion constructor

		#region property PoolItemID

		public ulong PoolItemID {
			get { return m_nPoolItemID; }
			set {
				m_nPoolItemID = value;
				SetName();
			} // set
		} // PoolItemID

		private ulong m_nPoolItemID;

		#endregion property PoolItemID

		#region property IsPooled

		public bool IsPooled {
			get { return m_bIsPooled; }
			set {
				m_bIsPooled = value;
				SetName();
			} // set
		} // IsPooled

		private bool m_bIsPooled;

		#endregion property IsPooled

		#region property Name

		public string Name {
			get { return m_sName; }
		} // Name

		private void SetName() {
			m_sName = string.Format("{1}-{0}", PoolItemID, IsPooled ? "pool" : "pool-free");
		} // SetName

		private string m_sName;

		#endregion property Name

		public DbConnection Connection { get; set; }
		public uint OutOfPoolCount { get; set; }
	} // class PooledConnection
} // namespace
