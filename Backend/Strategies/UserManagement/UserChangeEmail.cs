﻿namespace EzBob.Backend.Strategies.UserManagement {
	using System;
	using ConfigManager;
	using EZBob.DatabaseLib.Model.Email;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;
	using MailStrategies;

	public class UserChangeEmail : AStrategy {
		#region public

		#region constructor

		public UserChangeEmail(int nUserID, string sNewEmail, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			ErrorMessage = null;

			m_oSpUpdate = new SpUserChangeEmail(DB, Log) {
				Email = sNewEmail,
				UserID = nUserID,
			};

			m_oData = new UserSecurityData(this) {
				Email = sNewEmail,
				NewPassword = m_oSpUpdate.Password.RawValue,
			};
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "User change email"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Log.Debug("User '{0}': change email request...", m_oData.Email);

			ErrorMessage = null;

			m_oData.ValidateEmail();
			m_oData.ValidateNewPassword();

			ErrorMessage = m_oSpUpdate.ExecuteScalar<string>();

			Log.Debug(
				"User '{0}' email has{1} been changed. {2}",
				m_oData.Email,
				string.IsNullOrWhiteSpace(ErrorMessage) ? "" : " NOT",
				ErrorMessage
			);

			if (!string.IsNullOrWhiteSpace(ErrorMessage))
				return;

			new PasswordChanged(m_oSpUpdate.UserID, m_oSpUpdate.Password.RawValue, DB, Log).Execute();

            string sAddress = string.Format("<a href='{0}/confirm/{1}'>click here</a>", CurrentValues.Instance.CustomerSite.Value, m_oSpUpdate.RequestID);

			new SendEmailVerification(m_oSpUpdate.UserID, m_oData.Email, sAddress, DB, Log).Execute();
		} // Execute

		#endregion method Execute

		#region property ErrorMessage

		public string ErrorMessage { get; private set; } // ErrorMessage

		#endregion property ErrorMessage

		#endregion public

		#region private

		private readonly UserSecurityData m_oData;
		private readonly SpUserChangeEmail m_oSpUpdate;

		#region class SpUserChangeEmail

		private class SpUserChangeEmail : AStoredProc {
			#region constructor

			public SpUserChangeEmail(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				m_oRequestID = Guid.NewGuid();
			} // constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters() {
				return (UserID > 0) && !string.IsNullOrWhiteSpace(Email);
			} // HasValidParameters

			#endregion method HasValidParameters

			#region property UserID

			public int UserID { [UsedImplicitly] get; set; }

			#endregion property UserID

			#region property Email

			[UsedImplicitly]
			public string Email {
				get { return m_sEmail; } // get
				set {
					m_sEmail = value;
					Password = new SimplePassword(16, value);
				} // set
			} // Email

			private string m_sEmail;

			#endregion property Email

			#region property EzPassword

			[UsedImplicitly]
			public string EzPassword {
				get { return Password.Hash; }
				set { }
			} // EzPassword

			public SimplePassword Password;

			#endregion property EzPassword

			#region property RequestID

			[UsedImplicitly]
			public Guid RequestID {
				get { return m_oRequestID; }
				set { } // set
			} // RequestID

			private readonly Guid m_oRequestID;

			#endregion property RequestID

			#region property RequestState

			[UsedImplicitly]
			public string RequestState {
				get { return EmailConfirmationRequestState.Pending.ToString(); }
				set { }
			} // RequestState

			#endregion property RequestState

			#region property Now

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				set { }
			} // Now

			#endregion property Now
		} // class SpUserChangePassword

		#endregion class SpUserChangeEmail

		#endregion private
	} // class UserChangeEmail
} // namespace EzBob.Backend.Strategies.UserManagement
