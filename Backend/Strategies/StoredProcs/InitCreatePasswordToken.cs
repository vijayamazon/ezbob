namespace Ezbob.Backend.Strategies.StoredProcs {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class InitCreatePasswordTokenByUserID : AStoredProcedure {
		public InitCreatePasswordTokenByUserID(int userID, AConnection db, ASafeLog log) : base(db, log) {
			UserID = userID;
			Token = Guid.NewGuid();
		} // constructor

		public override bool HasValidParameters() {
			return UserID > 0;
		} // HasValidParameters

		public void Execute() {
			ConnectionWrapper cw = null;
			try {
				cw = DB.GetPersistent();

				cw.BeginTransaction();

				ExecuteNonQuery(cw);

				cw.Commit();
			} catch (Exception e) {
				Token = Guid.Empty;

				if (cw != null)
					cw.Rollback();

				Log.Alert(e, "Failed to initialize password change token for user {0}.", UserID);
			} // try
		} // Execute

		public Guid Token { get; set; }

		public int UserID { get; set; }

		public DateTime Now {
			get { return DateTime.UtcNow; }
			// ReSharper disable once ValueParameterNotUsed
			set { }
		} // Now
	} // class InitCreatePasswordToken
} // namespace
