namespace Ezbob.Database {
	using System.Data;
	using System.Data.Common;
	using Pool;

	public class ConnectionWrapper {
		public ConnectionWrapper(PooledConnection oPooled) {
			Connection = oPooled.Connection;
			Pooled = oPooled;
			IsOpen = false;
			Transaction = null;
		} // constructor

		public ConnectionWrapper Open() {
			if (IsOpen)
				return this;

			if (Connection == null)
				return this;

			if (Connection.State == ConnectionState.Closed)
				Connection.Open();

			IsOpen = true;

			return this;
		} // Open

		public void Close() {
			if (Transaction != null)
				throw new DbException("Cannot close connection wrapper: transaction is still active.");

			if (Connection != null)
				Connection.Dispose();

			Connection = null;
		} // Close

		public ConnectionWrapper BeginTransaction() {
			if (Transaction != null)
				return this;

			if (!IsOpen)
				return this;

			Transaction = Connection.BeginTransaction();

			return this;
		} // BeginTransaction

		public void Commit() {
			if (Transaction == null)
				return;

			Transaction.Commit();
			Transaction.Dispose();
			Transaction = null;

			Close();
		} // Commit

		public void Rollback() {
			if (Transaction == null)
				return;

			Transaction.Rollback();
			Transaction.Dispose();
			Transaction = null;

			Close();
		} // Rollback

		public DbConnection Connection { get; private set; }

		public bool IsOpen { get; private set; }

		public DbTransaction Transaction { get; private set; }

		public PooledConnection Pooled { get; private set; }
	} // class ConnectionWrapper
} // namespace
