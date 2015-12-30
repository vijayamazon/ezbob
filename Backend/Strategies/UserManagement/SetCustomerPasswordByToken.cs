namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;

	public class SetCustomerPasswordByToken : AStrategy {
		public SetCustomerPasswordByToken(Guid token, DasKennwort password, bool isBrokerLead) {
			CustomerID = 0;

			this.spDetails = new LoadUserDetailsByRestoreToken(DB, Log) {
				Token = token, 
				IsBrokerLead = isBrokerLead,
			};

			this.securityData = new UserSecurityData(this) {
				NewPassword = password.Decrypt(),
			};

			this.spSetNewPassword = new SpSetCustomerPasswordByToken(DB, Log) {
				Token = token,
				IsBrokerLead = isBrokerLead,
			};
		} // constructor

		public override string Name {
			get { return "SetCustomerPasswordByToken"; }
		} // Name

		public override void Execute() {
			this.spDetails.Load();

			if (this.spDetails.Data.UserID <= 0) {
				Log.Warn("Failed to find user by token {0}.", this.spDetails.Token);
				return;
			} // if

			this.securityData.ValidateNewPassword();

			this.spSetNewPassword.UserID = this.spDetails.Data.UserID;

			var pu = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

			HashedPassword hashed = pu.Generate(this.spDetails.Data.Email, this.securityData.NewPassword);

			this.spSetNewPassword.EzPassword = hashed.Password;
			this.spSetNewPassword.Salt = hashed.Salt;
			this.spSetNewPassword.CycleCount = hashed.CycleCount;

			CustomerID = this.spSetNewPassword.ExecuteScalar<int>();
		} // Execute

		public int CustomerID { get; private set; }

		private readonly SpSetCustomerPasswordByToken spSetNewPassword;
		private readonly UserSecurityData securityData;
		private readonly LoadUserDetailsByRestoreToken spDetails;

		private class LoadUserDetailsByRestoreToken : AStoredProcedure {
			public LoadUserDetailsByRestoreToken(AConnection db, ASafeLog log) : base(db, log) {
				this.isLoaded = false;

				Data = new UserData();
			} // constructor

			public override bool HasValidParameters() {
				return Token != Guid.Empty;
			} // HasValidParameters

			public Guid Token {
				get { return this.token; }
				set {
					this.token = value;
					this.isLoaded = false;
				} // set
			} // Token

			public bool IsBrokerLead {
				get { return this.isBrokerLead; }
				set {
					this.isBrokerLead = value;
					this.isLoaded = false;
				} // set
			} // IsBrokerLead

			public UserData Data { get; private set; }

			public void Load() {
				if (this.isLoaded)
					return;

				Data.Clear();

				FillFirst(Data);
				this.isLoaded = true;
			} // Load

			public class UserData {
				public UserData() {
					Clear();
				} // constructor

				public int UserID { get; set; }
				public string Email { get; set; }

				public void Clear() {
					UserID = 0;
					Email = null;
				} // Clear
			} // class UserData

			private bool isLoaded;
			private bool isBrokerLead;
			private Guid token;
		} // class LoadUserDetailsByRestoreToken

		private class SpSetCustomerPasswordByToken : AStoredProc {
			public SpSetCustomerPasswordByToken(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				return
					(UserID > 0) &&
					(Token != Guid.Empty) &&
					!string.IsNullOrWhiteSpace(EzPassword) &&
					!string.IsNullOrWhiteSpace(Salt) &&
					!string.IsNullOrWhiteSpace(CycleCount);
			} // HasValidParameters

			[UsedImplicitly]
			public int UserID { get; set; }

			[UsedImplicitly]
			public Guid Token { get; set; }

			[UsedImplicitly]
			public string EzPassword { get; set; }

			[UsedImplicitly]
			public string Salt { get; set; }

			[UsedImplicitly]
			public string CycleCount { get; set; }

			[UsedImplicitly]
			public bool IsBrokerLead { get; set; }

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // Now
		} // class SpSetCustomerPasswordByToken
	} // class SetCustomerPasswordByToken
} // namespace
