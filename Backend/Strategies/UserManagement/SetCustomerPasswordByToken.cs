namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.dbutils;
	using Ezbob.Utils.Security;
	using EZBob.DatabaseLib.Model.Database;
	using JetBrains.Annotations;

	public class SetCustomerPasswordByToken : AStrategy {
		public SetCustomerPasswordByToken(
			Guid token,
			CustomerOriginEnum origin,
			DasKennwort password,
			DasKennwort passwordAgain,
			bool isBrokerLead,
			string remoteIP
		) {
			UserID = 0;
			IsBroker = false;
			SessionID = 0;
			ErrorMsg = null;

			this.origin = origin;
			this.passwordAgain = passwordAgain.Decrypt();

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
				RemoteIP = remoteIP,
			};
		} // constructor

		public override string Name {
			get { return "SetCustomerPasswordByToken"; }
		} // Name

		public override void Execute() {
			this.spDetails.Load();

			if (this.spDetails.Data.UserID <= 0) {
				Log.Warn("Failed to find user by token {0}.", this.spDetails.Token);
				ErrorMsg = "Invalid password restore token.";
				return;
			} // if

			if ((this.spDetails.Data.OriginID == null) || (this.spDetails.Data.OriginID <= 0)) {
				Log.Warn(
					"User {1} (found by token {0}) has no proper origin set.",
					this.spDetails.Token,
					this.spDetails.Data.UserID
				);
				ErrorMsg = "Invalid password restore token.";
				return;
			} // if

			if ((int)this.origin != this.spDetails.Data.OriginID) {
				Log.Warn(
					"User {1} (found by token {0}) has origin '{2}' while source UI origin is '{3}'.",
					this.spDetails.Token,
					this.spDetails.Data.UserID,
					this.spDetails.Data.OriginID,
					(int)this.origin
				);
				ErrorMsg = "Invalid password restore token.";
				return;
			} // if

			if (!ValidatePassword())
				return;

			this.spSetNewPassword.UserID = this.spDetails.Data.UserID;
			UserID = this.spDetails.Data.UserID;

			var pu = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

			HashedPassword hashed = pu.Generate(this.spDetails.Data.Email, this.securityData.NewPassword);

			this.spSetNewPassword.EzPassword = hashed.Password;
			this.spSetNewPassword.Salt = hashed.Salt;
			this.spSetNewPassword.CycleCount = hashed.CycleCount;

			SafeReader sr = this.spSetNewPassword.GetFirst();

			if (sr.IsEmpty) {
				ErrorMsg = "Failed to set new password.";
				return;
			} // if

			IsBroker = sr["IsBroker"];
			SessionID = sr["SessionID"];
			IsDisabled = sr["IsDisabled"];

			if (IsDisabled) {
				ErrorMsg = string.Format(
					"This account is closed, please contact <span class='bold'>{0}</span> customer care<br/> {1}",
					sr["OriginName"],
					sr["CustomerCareEmail"]
				);
			} // if
		} // Execute

		public string ErrorMsg { get; private set; }
		public int UserID { get; private set; }
		public bool IsBroker { get; private set; }
		public bool IsDisabled { get; private set; }
		public string Email { get { return this.spDetails.Data.Email; } }
		public int SessionID { get; private set; }

		private bool ValidatePassword() {
			var maxPassLength = CurrentValues.Instance.PasswordPolicyType.Value == "hard" ? 7 : 6;

			if (this.securityData.NewPassword.Length < maxPassLength) {
				ErrorMsg = string.Format("Please enter a password that is {0} characters or more.", maxPassLength);
				return false;
			} // if

			try {
				this.securityData.ValidateNewPassword();
			} catch (Exception e) {
				ErrorMsg = e.Message;
				return false;
			} // try

			if (!string.Equals(this.securityData.NewPassword, this.passwordAgain)) {
				ErrorMsg = "Passwords don't match, please re-enter.";
				return false;
			} // if

			return true;
		} // ValidatePassword

		private readonly SpSetCustomerPasswordByToken spSetNewPassword;
		private readonly UserSecurityData securityData;
		private readonly LoadUserDetailsByRestoreToken spDetails;
		private readonly string passwordAgain;
		private readonly CustomerOriginEnum origin;

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
				[UsedImplicitly]
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

				public int UserID { get; [UsedImplicitly] set; }
				public string Email { get; [UsedImplicitly] set; }
				public int? OriginID { get; [UsedImplicitly] set; }

				public void Clear() {
					UserID = 0;
					Email = null;
					OriginID = null;
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
			[FieldName("Ip")]
			[Length(50)]
			public string RemoteIP { get; set; }

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
