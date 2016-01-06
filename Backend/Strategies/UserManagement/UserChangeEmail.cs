namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;
	using MailStrategies;
	using SalesForceLib;
	using SalesForceLib.Models;
	using StructureMap;

	public class UserChangeEmail : ASignupLoginBaseStrategy {
		public UserChangeEmail(int nUserID, string sNewEmail) {
			ErrorMessage = null;

			sNewEmail = NormalizeUserName(sNewEmail);

			this.spUpdate = new SpUserChangeEmail(DB, Log) {
				Email = sNewEmail,
				UserID = nUserID,
			};

			this.securityData = new UserSecurityData(this) {
				Email = sNewEmail,
				NewPassword = this.spUpdate.RawPassword,
			};
		} // constructor

		public override string Name {
			get { return "User change email"; }
		} // Name

		public override void Execute() {
			Log.Debug("User '{0}': request to change email to {1}...", this.spUpdate.UserID, this.securityData.Email);

			var customerData = (new CustomerData(this, this.spUpdate.UserID, DB));
			customerData.Load();

			this.securityData.ValidateEmail();
			this.securityData.ValidateNewPassword();

			ErrorMessage = this.spUpdate.ExecuteScalar<string>();

			Log.Debug(
				"User '{0}' email has{1} been changed. {2}",
				this.securityData.Email,
				string.IsNullOrWhiteSpace(ErrorMessage) ? "" : " NOT",
				ErrorMessage
			);

			if (!string.IsNullOrWhiteSpace(ErrorMessage))
				return;

			FireToBackground(new UpdateUploadedHmrcDisplayName(this.spUpdate.UserID));

			FireToBackground(new EmailChanged(this.spUpdate.UserID, this.spUpdate.RequestID.ToString()));

			ISalesForceAppClient salesForceApiClient = ObjectFactory
				.With("userName").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceUserName.Value)
				.With("password").EqualTo(ConfigManager.CurrentValues.Instance.SalesForcePassword.Value)
				.With("token").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceToken.Value)
				.With("environment").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceEnvironment.Value)
				.GetInstance<ISalesForceAppClient>();

			salesForceApiClient.ChangeEmail(new ChangeEmailModel{ currentEmail = customerData.Mail, newEmail = this.securityData.Email, Origin = customerData.Origin});
			Log.Debug(
				"User '{0}': request to change email to {1} fully processed.",
				this.spUpdate.UserID,
				this.securityData.Email
			);
		} // Execute

		public string ErrorMessage { get; private set; } // ErrorMessage

		private readonly UserSecurityData securityData;
		private readonly SpUserChangeEmail spUpdate;

		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		private class SpUserChangeEmail : AStoredProc {
			public SpUserChangeEmail(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				this.requestID = Guid.NewGuid();

				this.rawPassword = new DasKennwort();
				this.rawPassword.GenerateSimplePassword(16);

				this.passUtil = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);
				this.hashedPassword = this.passUtil.Generate(Email, RawPassword);
			} // constructor

			public override bool HasValidParameters() {
				return (UserID > 0) && !string.IsNullOrWhiteSpace(Email);
			} // HasValidParameters

			public int UserID { [UsedImplicitly] get; set; }

			[UsedImplicitly]
			public string Email {
				get { return this.email; } // get
				set {
					this.email = value;
					this.hashedPassword = this.passUtil.Generate(Email, RawPassword);
				} // set
			} // Email

			public string RawPassword { get { return this.rawPassword.Data; } }

			[UsedImplicitly]
			public string EzPassword {
				get { return this.hashedPassword.Password; }
				set { }
			} // EzPassword

			[UsedImplicitly]
			public string Salt {
				get { return this.hashedPassword.Salt; }
				set { }
			} // Salt

			[UsedImplicitly]
			public string CycleCount {
				get { return this.hashedPassword.CycleCount; }
				set { }
			} // CycleCount

			[UsedImplicitly]
			public Guid RequestID {
				get { return this.requestID; }
				set { } // set
			} // RequestID

			[UsedImplicitly]
			public string RequestState {
				get { return EmailConfirmationRequestState.Pending.ToString(); }
				set { }
			} // RequestState

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				set { }
			} // Now

			private readonly Guid requestID;
			private string email;
			private HashedPassword hashedPassword;
			private readonly DasKennwort rawPassword;
			private readonly PasswordUtility passUtil;
		} // class SpUserChangePassword
	} // class UserChangeEmail
} // namespace Ezbob.Backend.Strategies.UserManagement
