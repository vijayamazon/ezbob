namespace Ezbob.Database.Pool {
	using System;
	using Utils.ObjectPool;

	public class DbConnectionPool : ObjectPool<PooledConnection> {
		#region static constructor

		static DbConnectionPool() {
			ms_oLock = new object();
			ReuseCount = 10;
		} // static constructor

		#endregion static constructor

		#region property ReuseCount

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

		#endregion property ReuseCount

		#region constructor

		public DbConnectionPool() : base(100) {
		} // constructor

		#endregion constructor

		#region method Give

		public override PooledConnection Give() {
			PooledConnection pc = base.Give();
			pc.OutOfPoolCount++;
			return pc;
		} // Give

		#endregion method Give

		#region method Take

		public override bool Take(PooledConnection pc) {
			if (pc.OutOfPoolCount >= ReuseCount) {
				Drop(pc);
				return true;
			} // if

			return base.Take(pc);
		} // Take

		#endregion method Take

		#region method Drop

		public virtual void Drop(PooledConnection pc) {
			Log.Debug("An object (i.e. connection) {1}({0}) is dropped.", pc.PoolItemID, pc.Name);
			pc.Connection.Close();
			Forget(1);
		} // Take

		#endregion method Drop

		private static readonly object ms_oLock;
	} // class DbConnectionPool
} // namespace
