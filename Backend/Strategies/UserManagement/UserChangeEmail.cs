namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;
	using MailStrategies;
	using SalesForceLib;
	using StructureMap;

	public class UserChangeEmail : AStrategy {
		public UserChangeEmail(int nUserID, string sNewEmail) {
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

		public override string Name {
			get { return "User change email"; }
		} // Name

		public override void Execute() {
			Log.Debug("User '{0}': request to change email to {1}...", m_oSpUpdate.UserID, m_oData.Email);
			string oldEmail = DB.ExecuteScalar<string>("GetCustomerEmail", CommandSpecies.StoredProcedure, new QueryParameter("CustomerID", m_oSpUpdate.UserID));
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

			new System.Threading.Thread(() =>
				new EmailChanged(m_oSpUpdate.UserID, m_oSpUpdate.RequestID.ToString()).Execute()
			).Start();


			ISalesForceAppClient salesForceApiClient = ObjectFactory
				.With("userName").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceUserName.Value)
				.With("password").EqualTo(ConfigManager.CurrentValues.Instance.SalesForcePassword.Value)
				.With("token").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceToken.Value)
				.With("environment").EqualTo(ConfigManager.CurrentValues.Instance.SalesForceEnvironment.Value)
				.GetInstance<ISalesForceAppClient>();

			salesForceApiClient.ChangeEmail(oldEmail, m_oData.Email);
			Log.Debug("User '{0}': request to change email to {1} fully processed.", m_oSpUpdate.UserID, m_oData.Email);
		} // Execute

		public string ErrorMessage { get; private set; } // ErrorMessage

		private readonly UserSecurityData m_oData;
		private readonly SpUserChangeEmail m_oSpUpdate;

		private class SpUserChangeEmail : AStoredProc {

			public SpUserChangeEmail(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				m_oRequestID = Guid.NewGuid();
			} // constructor

			public override bool HasValidParameters() {
				return (UserID > 0) && !string.IsNullOrWhiteSpace(Email);
			} // HasValidParameters

			public int UserID { [UsedImplicitly] get; set; }

			[UsedImplicitly]
			public string Email {
				get { return m_sEmail; } // get
				set {
					m_sEmail = value;
					Password = new SimplePassword(16, value);
				} // set
			} // Email

			private string m_sEmail;

			// ReSharper disable ValueParameterNotUsed

			[UsedImplicitly]
			public string EzPassword {
				get { return Password.Hash; }
				set { }
			} // EzPassword

			public SimplePassword Password;

			[UsedImplicitly]
			public Guid RequestID {
				get { return m_oRequestID; }
				set { } // set
			} // RequestID

			private readonly Guid m_oRequestID;

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

			// ReSharper restore ValueParameterNotUsed
		} // class SpUserChangePassword

	} // class UserChangeEmail
} // namespace Ezbob.Backend.Strategies.UserManagement
