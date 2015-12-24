namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Web.Security;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Broker;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Backend.Strategies.UserManagement.EmailConfirmation;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Exceptions;
	using Ezbob.Utils.Security;
	using EZBob.DatabaseLib.Model.Database;
	using JetBrains.Annotations;

	public class SignupCustomerMutliOrigin : AStrategy {
		public SignupCustomerMutliOrigin(SignupCustomerMultiOriginModel model) {
			this.model = model;
		} // constructor

		public override string Name {
			get { return "SignupCustomerMutliOrigin"; }
		} // Name

		public override void Execute() {
			Success = false;

			if (this.model == null)
				throw new StrategyAlert(this, "No sign up data specified.");

			this.dbTransaction = DB.GetPersistent();

			try {
				CreateSecurityUserStaff();

				if (Status != MembershipCreateStatus.Success)
					throw new UserNotCreatedException();

				CreateCustomerStaff();

				this.dbTransaction.Commit();

				Success = true;
			} catch (Exception e) {
				Log.Warn(e, "Failed to create a customer with name '{0}'.", this.model.UserName);
				this.dbTransaction.Rollback();
			} // try

			if (Success)
				DoBrokerLeadsAndThirdParties();
		} // Execute

		public bool Success { get; private set; }
		public int UserID { get; private set; }
		public int SessionID { get; private set; }
		public int OriginID { get; private set; }
		public MembershipCreateStatus? Status { get; private set; }

		public string Result {
			get { return Status.HasValue ? Status.Value.ToString() : string.Empty;}
		} // Result

		private void DoBrokerLeadsAndThirdParties() {
			string token = GenerateConfirmationToken();

			if (token != null) {
				AStrategy stra = CreateEmailStrategy(token);

				if (stra != null)
					FireToBackground(string.Format("{0} for customer {1}", stra.Name, UserID), () => stra.Execute());
			} // if

			FireToBackground(
				"SalesForce add lead for customer " + this.model.UserName,
				() => new AddUpdateLeadAccount(this.model.UserName, UserID, false, false).Execute()
			);
		} // DoBrokerLeadsAndThirdParties

		private AStrategy CreateEmailStrategy(string token) {
			try {
				if (this.model.BrokerLeadIsSet()) {
					return new BrokerLeadAcquireCustomer(
						UserID,
						this.model.BrokerLeadID,
						this.model.BrokerLeadFirstName,
						this.model.BrokerFillsForCustomer,
						token
					);
				} // if

				return new BrokerCheckCustomerRelevance(
					UserID,
					this.model.UserName,
					this.model.IsAlibaba(),
					this.model.ReferenceSource,
					token
				);
			} catch (Exception e) {
				Log.Alert(
					e,
					"Failed to create BrokerLeadAcquireCustomer/BrokerCheckCustomerRelevance for customer {0}; " +
					"attachment to broker not checked, greeting email not sent.",
					this.model.UserName
				);

				return null;
			} // try
		} // CreateEmailStrategy

		private string GenerateConfirmationToken() {
			try {
				var ecg = new EmailConfirmationGenerate(UserID);
				ecg.Execute();
				return ecg.Token.ToString();
			} catch (Exception e) {
				Log.Alert(
					e,
					"Failed to create email confirmation token for customer '{0}'; " +
					"attachment to broker not checked, greeting email not sent.",
					this.model.UserName
				);

				return null;
			} // try
		} // GenerateConfirmationToken

		private void CreateSecurityUserStaff() {
			if (this.model.Origin == null)
				throw new StrategyAlert(this, "No origin specified.");

			if (this.model.PasswordQuestion == null)
				throw new StrategyWarning(this, "No security question specified.");

			try {
				var data = new UserSecurityData(this) {
					Email = this.model.UserName,
					NewPassword = this.model.RawPassword,
					PasswordQuestion = this.model.PasswordQuestion.Value,
					PasswordAnswer = this.model.PasswordAnswer,
				};

				data.ValidateEmail();
				data.ValidateNewPassword();

				var passUtil = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

				HashedPassword hashedPassword = passUtil.Generate(this.model.UserName, this.model.RawPassword);
				
				var sp = new CreateUserForCustomer(DB, Log) {
					OriginID = (int)this.model.Origin.Value,
					Email = this.model.UserName,
					EzPassword = hashedPassword.Password,
					Salt = hashedPassword.Salt,
					CycleCount = hashedPassword.CycleCount,
					SecurityQuestionID = this.model.PasswordQuestion,
					SecurityAnswer = this.model.PasswordAnswer,
					Ip = this.model.RemoteIp,
				};

				Status = MembershipCreateStatus.ProviderError;

				UserID = 0;

				sp.ForEachRowSafe(this.dbTransaction, (sr, bRowsetStart) => {
					if (!sr.ContainsField("UserID"))
						return ActionResult.Continue;

					UserID = sr["UserID"];
					SessionID = sr["SessionID"];
					OriginID = sr["OriginID"];
					return ActionResult.SkipAll;
				});

				if (UserID == -1) {
					Log.Warn("User with email {0} and origin {1} already exists.", this.model.UserName, this.model.Origin);
					Status = MembershipCreateStatus.DuplicateEmail;
				} else if (UserID == -2) {
					Log.Warn("Could not find role '{0}'.", sp.RoleName);
					Status = MembershipCreateStatus.ProviderError;
				} else if (UserID <= 0) {
					Log.Alert("CreateWebUser returned unexpected result {0}.", UserID);
					Status = MembershipCreateStatus.ProviderError;
				} else
					Status = MembershipCreateStatus.Success;
			} catch (AException) {
				Status = MembershipCreateStatus.ProviderError;
				throw;
			} catch (Exception e) {
				Log.Alert(e, "Failed to create user.");
				Status = MembershipCreateStatus.ProviderError;
			} // try
		} // CreateSecurityUserStaff

		private void CreateCustomerStaff() {
			bool mobilePhoneVerified = false;

			if (!this.model.CaptchaMode) {
				var vmc = new ValidateMobileCode(this.model.MobilePhone, this.model.MobileVerificationCode) {
					Transaction = this.dbTransaction
				};
				vmc.Execute();
				mobilePhoneVerified = vmc.IsValidatedSuccessfully();
			} // if

			var customer = new CreateCustomer(this, mobilePhoneVerified);
			customer.ExecuteNonQuery(this.dbTransaction);

			if (!this.model.BrokerFillsForCustomer)
				new CreateCampaignSourceRef(this).ExecuteNonQuery(this.dbTransaction);

			new CreateCustomerRequestedLoan(this).ExecuteNonQuery(this.dbTransaction);

			new SaveSourceRefHistory(
				UserID,
				this.model.ReferenceSource,
				this.model.VisitTimes,
				this.model.BrokerFillsForCustomer ? null : this.model.CampaignSourceRef
			) { Transaction = this.dbTransaction, } .Execute();

			if (this.model.IsAlibaba())
				new CreateAlibabaBuyer(this).ExecuteNonQuery(this.dbTransaction);
		} // CreateCustomerStaff

		private readonly SignupCustomerMultiOriginModel model;
		private ConnectionWrapper dbTransaction;

		private class UserNotCreatedException : Exception {} // class UserNotCreatedException

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		private class CreateUserForCustomer : AStoredProcedure {
			public CreateUserForCustomer(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				if (OriginID <= 0)
					return false;

				if (string.IsNullOrEmpty(Email))
					return false;

				if (string.IsNullOrEmpty(EzPassword))
					return false;

				if (string.IsNullOrEmpty(Salt))
					return false;

				if (string.IsNullOrEmpty(CycleCount))
					return false;

				if (SecurityQuestionID <= 0)
					return false;

				if (string.IsNullOrEmpty(SecurityAnswer))
					return false;

				return true;
			} // HasValidParameters

			public string Email { get; set; }

			public string EzPassword { get; set; }

			public string Salt { get; set; }

			public string CycleCount { get; set; }

			public int? SecurityQuestionID { get; set; }

			public string SecurityAnswer { get; set; }

			public string RoleName {
				get { return UserSecurityData.WebRole; }
				set { }
			} // RoleName

			public int BranchID {
				get { return 0; }
				set { }
			} // BranchID

			public int OriginID { get; set; }

			public string Ip {
				get { return this.ip ?? string.Empty; }
				set { this.ip = value ?? string.Empty; }
			} // Ip

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				set { }
			} // Now

			private string ip;
		} // class CreateUserForCustomer

		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class CreateCustomer : AStoredProcedure {
			public CreateCustomer(SignupCustomerMutliOrigin stra, bool mobilePhoneVerified) : base(stra.DB, stra.Log) {
				this.stra = stra;
				this.refNumber = new RefNumber(this.stra.UserID);
				this.mobilePhoneVerified = mobilePhoneVerified;
			} // constructor

			public override bool HasValidParameters() {
				return (CustomerID > 0) && (OriginID > 0) && !string.IsNullOrWhiteSpace(UserName);
			} // HasValidParameters

			[FieldName("Name")]
			public string UserName {
				get { return this.stra.model.UserName; }
				set { }
			} // Name

			[FieldName("Id")]
			public int CustomerID {
				get { return this.stra.UserID; }
				set { }
			} // CustomerID

			public string Status {
				get { return EZBob.DatabaseLib.Model.Database.Status.Registered.ToString(); }
				set { }
			} // Status

			public string RefNumber {
				get { return this.refNumber;  }
				set { }
			} // RefNumber

			public int WizardStep {
				get { return (int)DbConstants.WizardStepType.SignUp; }
				set { }
			} // WizardStep

			public int CollectionStatus {
				get { return (int)DbConstants.CollectionStatusNames.Enabled; }
				set { }
			} // CollectionStatus

			public bool? IsTest { // TODO: if null fill from email (TestCustomer table)
				get { return this.stra.model.IsTest; }
				set { }
			} // IsTest

			// IsOffline = null, // TODO: in DB
			// Vip = vip, // TODO: whether email exists in VipRequest
			// WhiteLabel = this.stra.model.WhiteLabelID, // TODO: validate exists in WhiteLabelProvider
			// Broker = broker, // TODO: find by white label

			public string MobilePhone {
				get { return this.stra.model.MobilePhone; }
				set { }
			} // MobilePhone

			public bool MobilePhoneVerified {
				get { return this.mobilePhoneVerified; }
				set { }
			} // MobilePhoneVerified

			public string FirstName {
				get { return NormalizeName(this.stra.model.FirstName); }
				set { }
			} // FirstName

			[FieldName("Surname")]
			public string LastName {
				get { return NormalizeName(this.stra.model.LastName); }
				set { }
			} // LastName

			public int TrustPilotStatus {
				get { return (int)EZBob.DatabaseLib.Model.Database.TrustPilotStauses.Neither; }
				set { }
			} // TrustPilotStatus

			public DateTime GreetingMailSentDate {
				get { return DateTime.UtcNow; }
				set { }
			} // GreetingMailSentDate

			public int OriginID {
				// ReSharper disable once PossibleInvalidOperationException
				// If Origin is null this class won't even be created.
				get { return (int)this.stra.model.Origin.Value; }
				set { }
			} // OriginID

			public string ABTesting {
				get { return this.stra.model.ABTesting; }
				set { }
			} // ABTesting

			public string FirstVisitTime {
				get { return this.stra.model.FirstVisitTime; }
				set { }
			} // FirstVisitTime

			public string ReferenceSource {
				get { return this.stra.model.BrokerFillsForCustomer ? "Broker" : this.stra.model.ReferenceSource; }
				set { }
			} // ReferenceSource

			public string GoogleCookie {
				get { return this.stra.model.BrokerFillsForCustomer ? string.Empty : this.stra.model.GoogleCookie; }
				set { }
			} // GoogleCookie

			public string AlibabaID {
				get { return this.stra.model.BrokerFillsForCustomer ? null : this.stra.model.AlibabaID; }
				set { }
			} // AlibabaID

			public bool? IsAlibaba {
				get {
					return this.stra.model.BrokerFillsForCustomer 
						? (bool?)null
						: !string.IsNullOrWhiteSpace(this.stra.model.AlibabaID);
				}
				set { }
			} // IsAlibaba

			private static string NormalizeName(string name) {
				var s = (name ?? string.Empty).Trim();
				return s == string.Empty ? null : s;
			} // NormalizeName

			private readonly bool mobilePhoneVerified;
			private readonly SignupCustomerMutliOrigin stra;
			private readonly string refNumber;
		} // class CreateCustomer

		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class CreateCampaignSourceRef : AStoredProcedure {
			public CreateCampaignSourceRef(SignupCustomerMutliOrigin stra) : base(stra.DB, stra.Log) {
				this.stra = stra;
			} // constructor

			public override bool HasValidParameters() {
				return (CustomerID > 0);
			} // HasValidParameters

			public int CustomerID { get { return this.stra.UserID; } set { } }

			public string FUrl { get { return this.stra.model.FUrl(); } set { } }
			public string FSource { get { return this.stra.model.FSource(); } set { } }
			public string FMedium { get { return this.stra.model.FMedium(); } set { } }
			public string FTerm { get { return this.stra.model.FTerm(); } set { } }
			public string FContent { get { return this.stra.model.FContent(); } set { } }
			public string FName { get { return this.stra.model.FName(); } set { } }
			public DateTime? FDate { get { return this.stra.model.FDate(); } set { } }
			public string RUrl { get { return this.stra.model.RUrl(); } set { } }
			public string RSource { get { return this.stra.model.RSource(); } set { } }
			public string RMedium { get { return this.stra.model.RMedium(); } set { } }
			public string RTerm { get { return this.stra.model.RTerm(); } set { } }
			public string RContent { get { return this.stra.model.RContent(); } set { } }
			public string RName { get { return this.stra.model.RName(); } set { } }
			public DateTime? RDate { get { return this.stra.model.RDate(); } set { } }

			private readonly SignupCustomerMutliOrigin stra;
		} // class CreateCampaignSourceRef

		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class CreateCustomerRequestedLoan : AStoredProcedure {
			public CreateCustomerRequestedLoan(SignupCustomerMutliOrigin stra) : base(stra.DB, stra.Log) {
				this.stra = stra;
			} // constructor

			public override bool HasValidParameters() {
				return (CustomerID > 0);
			} // HasValidParameters

			public int CustomerID { get { return this.stra.UserID; } set { } }

			public DateTime Created {
				get { return DateTime.UtcNow; }
				set { }
			} // Created

			public int Amount {
				get {
					return ToInt(
						this.stra.model.RequestedLoanAmount,
						this.stra.model.Origin == CustomerOriginEnum.everline ? 24000 : 20000
					);
				}
				set { }
			} // Amount

			public int Term {
				get {
					return ToInt(
						this.stra.model.RequestedLoanTerm,
						this.stra.model.Origin == CustomerOriginEnum.everline ? 12 : 9
					);
				}
				set { }
			} // Term

			private static int ToInt(string intStr, int intDefault) {
				if (string.IsNullOrEmpty(intStr))
					return intDefault;
				
				int result;

				bool bSuccess = int.TryParse(intStr, out result);

				return bSuccess ? result : intDefault;
			} // ToInt

			private readonly SignupCustomerMutliOrigin stra;
		} // class CreateCustomerRequestedLoan

		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class CreateAlibabaBuyer : AStoredProcedure {
			public CreateAlibabaBuyer(SignupCustomerMutliOrigin stra) : base(stra.DB, stra.Log) {
				this.stra = stra;
			} // constructor

			public override bool HasValidParameters() {
				return (CustomerID > 0);
			} // HasValidParameters

			public int CustomerID { get { return this.stra.UserID; } set { } }

			[FieldName("AliId")]
			public long AliID {
				get { return Convert.ToInt64(this.stra.model.AlibabaID); }
				set { }
			} // AliID

			private readonly SignupCustomerMutliOrigin stra;
		} // class CreateAlibabaBuyer
	} // class SignupAlibabaBuyer
} // namespace
