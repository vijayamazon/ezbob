namespace Ezbob.Database.Pool {
	using System;
	using Utils.ObjectPool;

	public class DbConnectionPool : ObjectPool<PooledConnection> {
		static DbConnectionPool() {
			ms_oLock = new object();
			ReuseCount = 10;
		} // static constructor

		public static int ReuseCount {
			get {
				lock (ms_oLock)
					return ms_nReuseCount;
			} // get
			set {
				lock (ms_oLock)
					ms_nReuseCount = Math.Max(value, 1);
			} // set
		} // ReuseCount

		private static int ms_nReuseCount;

		public DbConnectionPool() : base(100) {
		} // constructor

		public override PooledConnection Give() {
			PooledConnection pc = base.Give();
			pc.OutOfPoolCount++;
			return pc;
		} // Give

		public override bool Take(PooledConnection pc) {
			if (pc.OutOfPoolCount >= ReuseCount) {
				Drop(pc);
				return true;
			} // if

			return base.Take(pc);
		} // Take

		public virtual void Drop(PooledConnection pc) {
			// Log.Debug("An object (i.e. connection) {1}({0}) is dropped.", pc.PoolItemID, pc.Name);
			pc.Connection.Close();
			Forget(1);
		} // Take

		private static readonly object ms_oLock;
	} // class DbConnectionPool
} // namespace
