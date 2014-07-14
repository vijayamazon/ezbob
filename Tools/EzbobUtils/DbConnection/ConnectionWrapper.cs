namespace Ezbob.Database {
	using System;
	using System.Data.Common;

	public class ConnectionWrapper : IDisposable {
		#region constructor

		public ConnectionWrapper(DbConnection oConnection, bool bIsPersistent) {
			Connection = oConnection;
			IsPersistent = bIsPersistent;
			IsOpen = false;
		} // constructor

		#endregion constructor

		#region method Open

		public void Open() {
			if (IsOpen)
				return;

			if (Connection == null)
				return;

			Connection.Open();

			IsOpen = true;
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
			if (Connection != null)
				Connection.Dispose();

			Connection = null;
		} // Close

		#endregion method Close

		public DbConnection Connection { get; private set; }

		public bool IsPersistent { get; private set; }

		public bool IsOpen { get; private set; }
	} // class ConnectionWrapper
} // namespace
