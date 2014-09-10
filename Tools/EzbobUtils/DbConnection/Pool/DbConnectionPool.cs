namespace Ezbob.Database.Pool {
	using Utils.ObjectPool;

	internal class DbConnectionPool : ObjectPool<PooledConnection> {
		#region constructor

		public DbConnectionPool(int nMaxSize) : base(nMaxSize) {
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
			if (pc.OutOfPoolCount >= 10) {
				Drop(pc);
				return true;
			} // if

			return base.Take(pc);
		} // Take

		#endregion method Take

		#region method Drop

		public virtual void Drop(PooledConnection pc) {
			pc.Connection.Close();
			Forget(1);
		} // Take

		#endregion method Drop
	} // class DbConnectionPool
} // namespace
