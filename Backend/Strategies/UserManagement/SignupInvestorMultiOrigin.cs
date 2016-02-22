namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using System.ComponentModel;
	using ConfigManager;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Exceptions;
	using Ezbob.Utils.Extensions;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;

	public class SignupInvestorMultiOrigin : ASignupLoginBaseStrategy {
		public SignupInvestorMultiOrigin(string email) {
			string userName = NormalizeUserName(email);

			this.securityData = new UserSecurityData(this) {
				Email = userName,
				NewPassword = "123456",
			};

			var pu = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

			var pass = pu.Generate(email, this.securityData.NewPassword);

			this.sp = new CreateUserForInvestor(DB, Log) {
				UserName = userName,
				EzPassword = pass.Password,
				Salt = pass.Salt,
				CycleCount = pass.CycleCount,
			};

			UserID = 0;
		} // constructor

		public override string Name {
			get { return "Investor signup"; }
		} // Name

		public ConnectionWrapper Transaction { get; set; }

		public int UserID { get; private set; }

		public override void Execute() {
			try {
				this.securityData.ValidateEmail(true);
				this.securityData.ValidateNewPassword();

				UserID = 0;

				this.sp.ForEachRowSafe(Transaction, (sr, bRowsetStart) => {
					if (!sr.ContainsField("UserID"))
						return ActionResult.Continue;

					UserID = sr["UserID"];
					return ActionResult.SkipAll;
				});

				switch (UserID) {
				case (int)CreateUserForInvestor.Errors.DuplicateUser:
					throw new StrategyAlert(this, string.Format(
						"User with name {0} already exists.",
						this.securityData.Email
					));

				case (int)CreateUserForInvestor.Errors.RoleNotFound:
					throw new StrategyAlert(this, "Could not find role 'InvestorWeb'.");

				case (int)CreateUserForInvestor.Errors.FailedToCreateUser:
				case (int)CreateUserForInvestor.Errors.FailedToAttachRole:
				case (int)CreateUserForInvestor.Errors.ConflictsWithLead:
					throw new StrategyAlert(this, string.Format(
						"Internal DB error: {0}.",
						((CreateUserForInvestor.Errors)UserID).DescriptionAttr()
					));

				default:
					if (UserID <= 0) {
						throw new StrategyAlert(this, string.Format(
							"{0} returned unexpected result {1}.",
							this.sp.GetType().Name,
							UserID));
					} // if

					Log.Info("New underwriter was created.");

					break;
				} // switch
			} catch (AException) {
				throw;
			} catch (Exception e) {
				Log.Alert(e, "Failed to create user.");
				throw;
			} // try
		} // Execute

		private readonly UserSecurityData securityData;
		private readonly CreateUserForInvestor sp;

		private class CreateUserForInvestor : AStoredProcedure {
			public enum Errors {
				DuplicateUser         = -1,
				RoleNotFound          = -2,
				[Description("failed to create Security_User entry")]
				FailedToCreateUser    = -3,
				[Description("failed to attach user to security role")]
				FailedToAttachRole    = -4,
				[Description("conflicts with existing broker lead")]
				ConflictsWithLead     = -5,
			} // enum Errors

			public CreateUserForInvestor(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				return
					!string.IsNullOrWhiteSpace(UserName) &&
					!string.IsNullOrWhiteSpace(EzPassword) &&
					!string.IsNullOrWhiteSpace(Salt) &&
					!string.IsNullOrWhiteSpace(CycleCount);
			} // HasValidParameters

			public string UserName { [UsedImplicitly] get; set; }

			public string EzPassword { [UsedImplicitly] get; set; }
			public string Salt { [UsedImplicitly] get; set; }
			public string CycleCount { [UsedImplicitly] get; set; }

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				// ReSharper disable ValueParameterNotUsed
				set { }
				// ReSharper restore ValueParameterNotUsed
			} // Now
		} // class CreateUserForInvestor
	} // class SignupInvestorMultiOrigin
} // namespace

