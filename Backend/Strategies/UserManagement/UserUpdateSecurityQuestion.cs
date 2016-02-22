namespace Ezbob.Backend.Strategies.UserManagement {
	using System.Diagnostics.CodeAnalysis;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EZBob.DatabaseLib.Model.Database;

	public class UserUpdateSecurityQuestion : AStrategy {
		public UserUpdateSecurityQuestion(
			string email,
			CustomerOriginEnum? origin,
			DasKennwort password,
			int questionID,
			string answer
		) {
			ErrorMessage = null;

			this.securityData = new UserSecurityData(this) {
				Email = email,
				OldPassword = password.Decrypt(),
				PasswordQuestion = questionID,
				PasswordAnswer = answer,
			};

			this.spLoad = new UserDataForLogin(DB, Log) {
				Email = email,
				OriginID = (origin == null) ? (int?)null : (int)origin.Value,
			};

			this.spUpdate = new SpUserUpdateSecurityQuestion(DB, Log) {
				QuestionID = questionID,
				Answer = answer,
			};
		} // constructor

		public override string Name {
			get { return "User update security question"; }
		} // Name

		public string ErrorMessage { get; set; } // ErrorMessage

		public override void Execute() {
			ErrorMessage = null;

			string userDisplay = string.Format("{0} with origin {1}", this.securityData.Email, this.spLoad.OriginID);

			Log.Debug("User '{0}' tries to change security question...", userDisplay);

			this.securityData.ValidateEmail();
			this.securityData.ValidateOldPassword();

			var oUser = this.spLoad.FillFirst<UserDataForLogin.Result>();

			Log.Debug("User '{0}': data for check is {1}.", userDisplay, oUser);

			if (oUser.Email != this.securityData.Email) {
				Log.Debug("User '{0}' not found.", userDisplay);
				ErrorMessage = "User not found.";
				return;
			} // if

			this.spUpdate.UserID = oUser.UserID;

			if (!oUser.IsPasswordValid(this.securityData.OldPassword)) {
				Log.Debug("User '{0}' has supplied incorrect password.", userDisplay);
				ErrorMessage = "Incorrect password.";
				return;
			} // if

			if (oUser.RenewStoredPassword == null) {
				this.spUpdate.EzPassword = null;
				this.spUpdate.Salt = null;
				this.spUpdate.CycleCount = null;
			} else {
				this.spUpdate.EzPassword = oUser.RenewStoredPassword.Password;
				this.spUpdate.Salt = oUser.RenewStoredPassword.Salt;
				this.spUpdate.CycleCount = oUser.RenewStoredPassword.CycleCount;
			} // if

			ErrorMessage = this.spUpdate.ExecuteScalar<string>();

			Log.Debug(
				"User '{0}' security question has{1} been changed.",
				userDisplay,
				string.IsNullOrWhiteSpace(ErrorMessage) ? "" : " NOT"
			);
		} // Execute

		private readonly UserSecurityData securityData;
		private readonly UserDataForLogin spLoad;
		private readonly SpUserUpdateSecurityQuestion spUpdate;

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
		private class SpUserUpdateSecurityQuestion : AStoredProc {
			public SpUserUpdateSecurityQuestion(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				if ((UserID <= 0) || (QuestionID <= 0) || string.IsNullOrEmpty(Answer))
					return false;

				bool hasPassword =
					!string.IsNullOrWhiteSpace(EzPassword) &&
					!string.IsNullOrWhiteSpace(Salt) &&
					!string.IsNullOrWhiteSpace(CycleCount);

				bool noPassword =
					string.IsNullOrWhiteSpace(EzPassword) &&
					string.IsNullOrWhiteSpace(Salt) &&
					string.IsNullOrWhiteSpace(CycleCount);

				return hasPassword || noPassword;
			} // HasValidParameters

			public int UserID { get; set; }

			public long QuestionID { get; set; }

			public string Answer { get; set; }

			public string EzPassword { get; set; }
			public string Salt { get; set; }
			public string CycleCount { get; set; }
		} // class SpUserUpdateSecurityQuestion
	} // class UserUpdateSecurityQuestion
} // namespace Ezbob.Backend.Strategies.UserManagement
