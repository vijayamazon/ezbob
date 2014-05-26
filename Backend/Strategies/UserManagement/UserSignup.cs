﻿namespace EzBob.Backend.Strategies.UserManagement {
	using System;
	using System.Web.Security;
	using Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class UserSignup : AStrategy {
		#region public

		#region constructor

		// Create a user for Customer
		public UserSignup(
			string sEmail,
			string sPassword,
			int nPasswordQuestion,
			string sPasswordAnswer,
			string sRemoteIp,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_oResult = null;

			m_oData = new UserSecurityData(this) {
				Email = sEmail,
				NewPassword = sPassword,
				PasswordQuestion = nPasswordQuestion,
				PasswordAnswer = sPasswordAnswer,
			};

			m_oSp = new CreateWebUser(DB, Log) {
				Email = m_oData.Email,
				EzPassword = Ezbob.Utils.Security.SecurityUtils.HashPassword(m_oData.Email, m_oData.NewPassword),
				SecurityQuestionID = m_oData.PasswordQuestion,
				SecurityAnswer = m_oData.PasswordAnswer,
				RoleName = "Web",
				BranchID = 0,
				Ip = sRemoteIp,
			};
		} // constructor

		// Create a user for Underwriter
		public UserSignup(string sEmail, string sPassword, string sRoleName, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oResult = null;

			m_oData = new UserSecurityData(this) {
				Email = sEmail,
				NewPassword = sPassword,
			};

			m_oSp = new CreateWebUser(DB, Log) {
				Email = m_oData.Email,
				EzPassword = Ezbob.Utils.Security.SecurityUtils.HashPassword(m_oData.Email, m_oData.NewPassword),
				SecurityQuestionID = m_oData.PasswordQuestion,
				SecurityAnswer = m_oData.PasswordAnswer,
				RoleName = sRoleName,
				BranchID = 1,
			};
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Create user"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oResult = MembershipCreateStatus.ProviderError;

			try {
				m_oData.ValidateEmail();
				m_oData.ValidateNewPassword();

				int nUserID = 0;

				m_oSp.ForEachRowSafe((sr, bRowsetStart) => {
					if (!sr.ContainsField("UserID"))
						return ActionResult.Continue;

					nUserID = sr["UserID"];
					SessionID = sr["SessionID"];
					return ActionResult.SkipAll;
				});

				if (nUserID == -1) {
					Log.Warn("User with email {0} already exists.", m_oData.Email);
					m_oResult = MembershipCreateStatus.DuplicateEmail;
				}
				else if (nUserID == -2) {
					Log.Warn("Could not find role '{0}'.", m_oSp.RoleName);
					m_oResult = MembershipCreateStatus.ProviderError;
				}
				else if (nUserID <= 0) {
					Log.Alert("CreateWebUser returned unexpected result {0}.", nUserID);
					m_oResult = MembershipCreateStatus.ProviderError;
				}
				else
					m_oResult = MembershipCreateStatus.Success;
			}
			catch (AStrategyException) {
				m_oResult = MembershipCreateStatus.ProviderError;
				throw;
			}
			catch (Exception e) {
				Log.Alert(e, "Failed to create user.");
				m_oResult = MembershipCreateStatus.ProviderError;
			} // try
		} // Execute

		#endregion method Execute

		#region property Result

		public string Result {
			get { return m_oResult.HasValue ? m_oResult.Value.ToString() : string.Empty; } // get
		} // Result

		#endregion property Result

		#region property SessionID

		public int SessionID { get; private set; } // SessionID

		#endregion property SessionID

		#endregion public

		#region private

		private MembershipCreateStatus? m_oResult;
		private readonly UserSecurityData m_oData;
		private readonly CreateWebUser m_oSp;

		#region class CreateWebUser

		private class CreateWebUser : AStoredProcedure {
			public CreateWebUser(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return
					!string.IsNullOrEmpty(Email) &&
					!string.IsNullOrEmpty(EzPassword) &&
					(SecurityQuestionID > 0) &&
					!string.IsNullOrEmpty(SecurityAnswer);
			} // HasValidParameters

			public string Email { [UsedImplicitly] get; set; }

			public string EzPassword { [UsedImplicitly] get; set; }

			public int SecurityQuestionID { [UsedImplicitly] get; set; }

			public string SecurityAnswer { [UsedImplicitly] get; set; }

			public string RoleName { [UsedImplicitly] get; set; }

			public int BranchID { [UsedImplicitly] get; set; }

			public string Ip {
				[UsedImplicitly]
				get { return m_sIp ?? string.Empty; }
				set { m_sIp = value ?? string.Empty; }
			} // Ip

			private string m_sIp;

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				set { }
			} // Now
		} // class CreateWebUser

		#endregion class CreateWebUser

		#endregion private
	} // class UserSignup
} // namespace EzBob.Backend.Strategies.UserManagement
