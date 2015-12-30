namespace Ezbob.Backend.Strategies.StoredProcs {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class InitCreatePasswordTokenByUserID : ATiedStoredProcedure {
		public InitCreatePasswordTokenByUserID(int userID, AConnection db, ASafeLog log) : base(db, log) {
			this.userID = userID;
			Token = Guid.NewGuid();
		} // constructor

		public override bool HasValidParameters() {
			return this.userID > 0;
		} // HasValidParameters

		protected override void TiedAction() {
			ConnectionWrapper cw = null;
			try {
				cw = DB.GetPersistent();

				cw.BeginTransaction();

				DB.ExecuteNonQuery(
					cw,
					GetName(),
					CommandSpecies.StoredProcedure,
					new QueryParameter("TokenID", Token),
					new QueryParameter("UserID", this.userID),
					new QueryParameter("Now", DateTime.UtcNow)
				);

				cw.Commit();
			} catch (Exception e) {
				Token = Guid.Empty;

				if (cw != null)
					cw.Rollback();

				Log.Alert(e, "Failed to initialize password change token for user {0}.", this.userID);
			} // try
		} // TiedAction

		public Guid Token { get; private set; }

		private readonly int userID;
	} // class InitCreatePasswordToken
} // namespace
