﻿namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using System.Web.Security;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Exceptions;
	using JetBrains.Annotations;

	public class UserSignup : AStrategy {

		// Create a user for Customer
		public UserSignup(
			string sEmail,
			Password oPassword,
			int nPasswordQuestion,
			string sPasswordAnswer,
			string sRemoteIp
		) {
			m_oResult = null;

			m_oData = new UserSecurityData(this) {
				Email = sEmail,
				NewPassword = oPassword.Primary,
				PasswordQuestion = nPasswordQuestion,
				PasswordAnswer = sPasswordAnswer,
			};

			m_oSp = new CreateWebUser(DB, Log) {
				Email = m_oData.Email,
				EzPassword = Ezbob.Utils.Security.SecurityUtils.HashPassword(m_oData.Email, m_oData.NewPassword),
				SecurityQuestionID = m_oData.PasswordQuestion,
				SecurityAnswer = m_oData.PasswordAnswer,
				RoleName = UserSecurityData.WebRole,
				BranchID = 0,
				Ip = sRemoteIp,
			};
		} // constructor

		// Create a user for Underwriter / investor
		// branch id: 1 - UW, 2 - Investor
		public UserSignup(string sEmail, string sPassword, string sRoleName, int branchID) {
			b_isUW = true;

			m_oResult = null;

			m_oData = new UserSecurityData(this) {
				Email = sEmail,
				NewPassword = sPassword,
			};

			m_oSp = new CreateWebUser(DB, Log) {
				Email = m_oData.Email,
				EzPassword = Ezbob.Utils.Security.SecurityUtils.HashPassword(m_oData.Email, m_oData.NewPassword),
				SecurityQuestionID = null,
				SecurityAnswer = null,
				RoleName = sRoleName,
				BranchID = branchID,
			};
		} // constructor

		public override string Name {
			get { return "Create user"; }
		} // Name

		public override void Execute() {
			m_oResult = MembershipCreateStatus.ProviderError;

			try {
				m_oData.ValidateEmail(b_isUW);
				m_oData.ValidateNewPassword();

				UserID = 0;

				m_oSp.ForEachRowSafe(ConnectionWrapper, (sr, bRowsetStart) => {
					if (!sr.ContainsField("UserID"))
						return ActionResult.Continue;

					UserID = sr["UserID"];
					SessionID = sr["SessionID"];
					OriginID = sr["OriginID"];
					return ActionResult.SkipAll;
				});

				if (UserID == -1) {
					Log.Warn("User with email {0} already exists.", m_oData.Email);
					m_oResult = MembershipCreateStatus.DuplicateEmail;
				}
				else if (UserID == -2) {
					Log.Warn("Could not find role '{0}'.", m_oSp.RoleName);
					m_oResult = MembershipCreateStatus.ProviderError;
				}
				else if (UserID <= 0) {
					Log.Alert("CreateWebUser returned unexpected result {0}.", UserID);
					m_oResult = MembershipCreateStatus.ProviderError;
				}
				else
					m_oResult = MembershipCreateStatus.Success;
			}
			catch (AException) {
				m_oResult = MembershipCreateStatus.ProviderError;
				throw;
			}
			catch (Exception e) {
				Log.Alert(e, "Failed to create user.");
				m_oResult = MembershipCreateStatus.ProviderError;
			} // try
		} // Execute

		public MembershipCreateStatus? Status { get {  return this.m_oResult; } }
		public string Result {
			get { return m_oResult.HasValue ? m_oResult.Value.ToString() : string.Empty; } // get
		} // Result

		public int UserID { get; private set; }

		public int OriginID { get; private set; } // OriginID
		public int SessionID { get; private set; } // SessionID
		public ConnectionWrapper ConnectionWrapper { get; set; }
		private MembershipCreateStatus? m_oResult;
		private readonly UserSecurityData m_oData;
		private readonly CreateWebUser m_oSp;
		private bool b_isUW;

		private class CreateWebUser : AStoredProcedure {
			public CreateWebUser(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(EzPassword))
					return false;

				if (RoleName.Equals(UserSecurityData.WebRole, StringComparison.InvariantCultureIgnoreCase)) {
					return
						(SecurityQuestionID > 0) &&
						!string.IsNullOrEmpty(SecurityAnswer);
				} // if

				return true;
			} // HasValidParameters

			public string Email { [UsedImplicitly] get; set; }

			public string EzPassword { [UsedImplicitly] get; set; }

			public int? SecurityQuestionID { [UsedImplicitly] get; set; }

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
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // Now
		} // class CreateWebUser

	} // class UserSignup
} // namespace Ezbob.Backend.Strategies.UserManagement
