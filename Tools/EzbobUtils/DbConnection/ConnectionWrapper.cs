namespace Ezbob.Database {
	using System;
	using System.Data.Common;

	public class ConnectionWrapper : IDisposable {
		#region constructor

		public ConnectionWrapper(DbConnection oConnection, bool bIsPersistent) {
			Connection = oConnection;
			IsPersistent = bIsPersistent;
			IsOpen = false;
			Transaction = null;
		} // constructor

		#endregion constructor

		#region method Open

		public ConnectionWrapper Open() {
			if (IsOpen)
				return this;

			if (Connection == null)
				return this;

			Connection.Open();

			IsOpen = true;

			return this;
		} // Open

		#endregion method Open

		#region method Dispose

		public void Dispose() {
			if (!IsPersistent)
				Close();
		} // Dispose

		#endregion method Dispose

		#region method Close

		public void Close() {
			if (Transaction != null)
				throw new DbException("Cannot close connection wrapper: transaction is still active.");

			if (Connection != null)
				Connection.Dispose();

			Connection = null;
		} // Close

		#endregion method Close

		#region method BeginTransaction

		public void BeginTransaction() {
			if (Transaction != null)
				return;

			if (!IsOpen)
				return;

			Transaction = Connection.BeginTransaction();
		} // BeginTransaction

		#endregion method BeginTransaction

		#region method Commit

		public void Commit() {
			if (Transaction == null)
				return;

			Transaction.Commit();
			Transaction.Dispose();
			Transaction = null;

			Close();
		} // Commit

		#endregion method Commit

		#region method Rollback

		public void Rollback() {
			if (Transaction == null)
				return;

			Transaction.Rollback();
			Transaction.Dispose();
			Transaction = null;

			Close();
		} // Rollback

		#endregion method Rollback

		public DbConnection Connection { get; private set; }

		public bool IsPersistent { get; private set; }

		public bool IsOpen { get; private set; }

		public DbTransaction Transaction { get; private set; }
	} // class ConnectionWrapper
} // namespace
