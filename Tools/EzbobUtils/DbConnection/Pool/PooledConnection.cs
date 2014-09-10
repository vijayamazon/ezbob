namespace Ezbob.Database.Pool {
	using System.Data.Common;
	using Ezbob.Utils.ObjectPool;

	public class PooledConnection : IPoolable {
		public PooledConnection() {
			OutOfPoolCount = 0;
		} // constructor

		public ulong PoolItemID { get; set; }

		public DbConnection Connection { get; set; }
		public uint OutOfPoolCount { get; set; }
	} // class PooledConnection
} // namespace
