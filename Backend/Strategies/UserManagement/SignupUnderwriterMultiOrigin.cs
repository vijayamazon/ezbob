namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using System.ComponentModel;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Exceptions;
	using Ezbob.Utils.Extensions;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;

	public class SignupUnderwriterMultiOrigin : ASignupLoginBaseStrategy {
		public SignupUnderwriterMultiOrigin(string sEmail, DasKennwort sPassword, string sRoleName) {
			string userName = NormalizeUserName(sEmail);

			this.securityData = new UserSecurityData(this) {
				Email = userName,
				NewPassword = sPassword.Decrypt(),
			};

			var pu = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

			var pass = pu.Generate(sEmail, this.securityData.NewPassword);

			this.sp = new CreateUserForUnderwriter(DB, Log) {
				UserName = userName,
				EzPassword = pass.Password,
				Salt = pass.Salt,
				CycleCount = pass.CycleCount,
				RoleName = (sRoleName ?? string.Empty).Trim().ToLowerInvariant(),
			};
		} // constructor

		public override string Name {
			get { return "Underwriter signup"; }
		} // Name

		public override void Execute() {
			try {
				this.securityData.ValidateEmail(true);
				this.securityData.ValidateNewPassword();

				int nUserID = 0;

				this.sp.ForEachRowSafe((sr, bRowsetStart) => {
					if (!sr.ContainsField("UserID"))
						return ActionResult.Continue;

					nUserID = sr["UserID"];
					return ActionResult.SkipAll;
				});

				switch (nUserID) {
				case (int)CreateUserForUnderwriter.Errors.DuplicateUser:
					Log.Warn("User with name {0} already exists.", this.securityData.Email);
					break;

				case (int)CreateUserForUnderwriter.Errors.RoleNotFound:
					Log.Warn("Could not find role '{0}'.", this.sp.RoleName);
					break;

				case (int)CreateUserForUnderwriter.Errors.FailedToCreateUser:
				case (int)CreateUserForUnderwriter.Errors.FailedToAttachRole:
				case (int)CreateUserForUnderwriter.Errors.FailedToCreateSession:
					Log.Alert(
						"Internal DB error: {0}.",
						((CreateUserForUnderwriter.Errors)nUserID).DescriptionAttr()
					);
					break;

				default:
					if (nUserID <= 0)
						Log.Alert("{0} returned unexpected result {1}.", this.sp.GetType().Name, nUserID);
					else
						Log.Info("New underwriter was created.");

					break;
				} // switch
			} catch (AException) {
				throw;
			} catch (Exception e) {
				Log.Alert(e, "Failed to create user.");
			} // try
		} // Execute

		private readonly UserSecurityData securityData;
		private readonly CreateUserForUnderwriter sp;

		private class CreateUserForUnderwriter : AStoredProcedure {
			public enum Errors {
				DuplicateUser         = -1,
				RoleNotFound          = -2,
				[Description("failed to create Security_User entry")]
				FailedToCreateUser    = -3,
				[Description("failed to attach user to security role")]
				FailedToAttachRole    = -4,
				[Description("failed to create CustomerSession entry")]
				FailedToCreateSession = -5,
			} // enum Errors

			public CreateUserForUnderwriter(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				return
					!string.IsNullOrWhiteSpace(UserName) &&
					!string.IsNullOrWhiteSpace(EzPassword) &&
					!string.IsNullOrWhiteSpace(Salt) &&
					!string.IsNullOrWhiteSpace(CycleCount) &&
					!string.IsNullOrWhiteSpace(RoleName);
			} // HasValidParameters

			public string UserName { [UsedImplicitly] get; set; }

			public string EzPassword { [UsedImplicitly] get; set; }
			public string Salt { [UsedImplicitly] get; set; }
			public string CycleCount { [UsedImplicitly] get; set; }

			public string RoleName { [UsedImplicitly] get; set; }

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // Now
		} // class CreateUserForUnderwriter
	} // class SignupUnderwriterMultiOrigin
} // namespace
