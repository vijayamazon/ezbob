namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;

	public class UserResetPassword : AStrategy {
		public UserResetPassword(int userID) {
			Password = new DasKennwort();
			Password.GenerateSimplePassword(8);

			this.userID = userID;
		} // constructor

		public override string Name {
			get { return "User reset password"; }
		} // Name

		public DasKennwort Password { get; private set; } // Password

		public bool Success { get; private set; } // Success

		public override void Execute() {
			Success = false;

			SafeReader sr = DB.GetFirst(
				"LoadUserDetails",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@UserID", this.userID)
			);

			if (sr.IsEmpty) {
				Log.Alert("User not found by id {0}.", this.userID);
				return;
			} // if

			var pu = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

			HashedPassword hashed = pu.Generate(sr["Email"], Password.Data);

			var sp = new SpUserResetPassword(DB, Log) {
				UserID = this.userID,
				EzPassword = hashed.Password,
				Salt = hashed.Salt,
				CycleCount = hashed.CycleCount,
			};

			Success = 0 < sp.ExecuteScalar<int>();
		} // Execute

		private readonly int userID;

		private class SpUserResetPassword : AStoredProc {
			public SpUserResetPassword(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return
					(UserID > 0) &&
					!string.IsNullOrWhiteSpace(EzPassword) &&
					!string.IsNullOrWhiteSpace(Salt) &&
					!string.IsNullOrWhiteSpace(CycleCount);
			} // HasValidParameters

			[UsedImplicitly]
			public int UserID { get; set; }

			[UsedImplicitly]
			public string EzPassword { get; set; }

			[UsedImplicitly]
			public string Salt { get; set; }

			[UsedImplicitly]
			public string CycleCount { get; set; }

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				// ReSharper disable once ValueParameterNotUsed
				set { }
			} // Now
		} // class SpUserResetPassword
	} // class UserResetPassword
} // namespace Ezbob.Backend.Strategies.UserManagement
