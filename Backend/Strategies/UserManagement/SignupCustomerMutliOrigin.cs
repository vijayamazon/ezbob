namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.Linq;
	using System.Web;
	using System.Web.Security;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.Strategies.Broker;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Backend.Strategies.UserManagement.EmailConfirmation;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.dbutils;
	using Ezbob.Utils.Exceptions;
	using Ezbob.Utils.Extensions;
	using Ezbob.Utils.Security;
	using EZBob.DatabaseLib.Model.Database;
	using JetBrains.Annotations;

	public class SignupCustomerMultiOrigin : ASignupLoginBaseStrategy {
		public SignupCustomerMultiOrigin(SignupCustomerMultiOriginModel model) {
			this.uniqueID = GenerateUniqueID();

			Status = MembershipCreateStatus.ProviderError;
			Success = false;
			this.dbTransaction = null;
			ErrorMsg = null;

			this.model = model;
		} // constructor

		public override string Name {
			get { return "SignupCustomerMultiOrigin"; }
		} // Name

		public override void Execute() {
			Log.Debug("Sign up attempt '{0}' started...", this.uniqueID);

			string userName = (this.model == null) ? "unknown name" : this.model.UserName;

			try {
				if (this.model == null) {
					SetInternalErrorMsg();
					Log.Alert("Sign up attempt '{0}': no sign up data specified.", this.uniqueID);

					throw new BadDataException();
				} // if

				Log.Debug("Sign up attempt '{0}', model is {1}.", this.uniqueID, this.model.ToLogStr());

				if (string.IsNullOrWhiteSpace(this.model.UserName)) {
					SetInternalErrorMsg("This is not a valid email address.");
					Log.Alert("Sign up attempt '{0}': no user name specified.", this.uniqueID);
					throw new BadDataException();
				} // if

				string rawPassword = this.model.RawPassword.Decrypt();
				string rawPasswordAgain = this.model.RawPasswordAgain.Decrypt();

				if (string.IsNullOrWhiteSpace(rawPassword)) {
					SetInternalErrorMsg("This is not a valid password.");
					Log.Alert("Sign up attempt '{0}': no password specified.", this.uniqueID);
					throw new BadDataException();
				} // if

				var maxPassLength = CurrentValues.Instance.PasswordPolicyType.Value == "hard" ? 7 : 6;

				if (rawPassword.Length < maxPassLength) {
					SetInternalErrorMsg(string.Format(
						"Please enter a password that is {0} characters or more.",
						maxPassLength
					));

					Log.Alert("Sign up attempt '{0}': password is too short.", this.uniqueID);
					throw new BadDataException();
				} // if

				if (rawPassword != rawPasswordAgain) {
					SetInternalErrorMsg("Passwords don't match, please re-enter.");
					Log.Alert("Sign up attempt '{0}': password mismatch.", this.uniqueID);
					throw new BadDataException();
				} // if

				this.dbTransaction = DB.GetPersistentTransaction();

				CreateSecurityUserStuff();

				if (Status != MembershipCreateStatus.Success)
					throw new UserNotCreatedException();

				CreateCustomerStuff();

				this.dbTransaction.Commit();

				Success = true;

				if (Success)
					FireToBackground("do broker leads and third-parties", DoBrokerLeadsAndThirdParties);

				Log.Info("Sign up attempt '{0}' completed, success: {1}.", this.uniqueID, Success ? "yes" : "no");
			} catch (Exception e) {
				Success = false;

				Log.Warn(e, "Sign up attempt {0} failed for customer name '{1}'.", this.uniqueID, userName);

				if (this.dbTransaction != null)
					this.dbTransaction.Rollback();

				Log.Info("Sign up attempt '{0}' completed with exception.", this.uniqueID);
			} // try
		} // Execute

		public bool Success { get; private set; }
		public int UserID { get; private set; }
		public int SessionID { get; private set; }
		public string ErrorMsg { get; private set; }
		public string RefNumber { get; private set; }
		public MembershipCreateStatus Status { get; private set; }

		private void DoBrokerLeadsAndThirdParties() {
			string token = GenerateConfirmationToken();

			if (token != null)
				FireToBackground(CreateEmailStrategy(token));

			FireToBackground(new AddUpdateLeadAccount(this.model.UserName, UserID, false, false));
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

		private void CreateSecurityUserStuff() {
			if (this.model.Origin == null) {
				SetInternalErrorMsg();
				Log.Alert("Sign up attempt {0}: no origin specified.", this.uniqueID);
				throw new BadDataException();
			} // if

			if (this.model.PasswordQuestion == null) {
				SetInternalErrorMsg();
				Log.Alert("Sign up attempt {0}: no security question specified.", this.uniqueID);
				throw new BadDataException();
			} // if

			try {
				string rawPassword = this.model.RawPassword.Decrypt();

				var data = new UserSecurityData(this) {
					Email = this.model.UserName,
					NewPassword = rawPassword,
					PasswordQuestion = this.model.PasswordQuestion.Value,
					PasswordAnswer = this.model.PasswordAnswer,
				};

				Log.Debug("Sign up attempt '{0}': validating user name...", this.uniqueID);

				data.ValidateEmail();

				Log.Debug("Sign up attempt '{0}': validating password...", this.uniqueID);

				data.ValidateNewPassword();

				Log.Debug("Sign up attempt '{0}': validated user name and password.", this.uniqueID);

				var passUtil = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

				HashedPassword hashedPassword = passUtil.Generate(this.model.UserName, rawPassword);
				
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

				UserID = 0;

				sp.ForEachRowSafe(this.dbTransaction, (sr, bRowsetStart) => {
					if (!sr.ContainsField("UserID"))
						return ActionResult.Continue;

					UserID = sr["UserID"];
					SessionID = sr["SessionID"];
					return ActionResult.SkipAll;
				});

				Status = MembershipCreateStatus.ProviderError;

				switch (UserID) {
				case (int)CreateUserForCustomer.Errors.DuplicateUser:
					ErrorMsg =
						"This email address already exists in our system. " +
						"Please try to log-in or request new password.";

					Status = MembershipCreateStatus.DuplicateEmail;

					Log.Warn(
						"Sign up attempt '{0}': user with email {1} and origin {2} already exists.",
						this.uniqueID,
						this.model.UserName,
						this.model.Origin.Value
					);

					break;

				case (int)CreateUserForCustomer.Errors.OriginNotFound:
					Log.Alert("Sign up attempt '{0}': origin {1} was not found.", this.uniqueID, this.model.Origin.Value);
					SetInternalErrorMsg();
					break;

				case (int)CreateUserForCustomer.Errors.RoleNotFound:
				case (int)CreateUserForCustomer.Errors.FailedToCreateUser:
				case (int)CreateUserForCustomer.Errors.FailedToAttachRole:
				case (int)CreateUserForCustomer.Errors.FailedToCreateSession:
				case (int)CreateUserForCustomer.Errors.ConflictsWithInternal:
				case (int)CreateUserForCustomer.Errors.ConflictsWithBroker:
					Log.Alert(
						"Sign up attempt '{0}' - internal DB error: {1}.",
						this.uniqueID,
						((CreateUserForCustomer.Errors)UserID).DescriptionAttr()
					);
					SetInternalErrorMsg();
					break;

				default:
					if (UserID <= 0) {
						Log.Alert(
							"Sign up attempt '{0}': {1} returned unexpected result: {2}.",
							this.uniqueID,
							sp.GetType().Name,
							UserID
						);
						SetInternalErrorMsg();
					} else {
						Log.Msg(
							"Sign up attempt '{0}': user '{1}' with origin {2} was inserted into Security_User table.",
							this.uniqueID,
							this.model.UserName,
							this.model.Origin.Value
						);
						Status = MembershipCreateStatus.Success;
					} // if

					break;
				} // switch
			} catch (AException ae) {
				SetInternalErrorMsg();
				Log.Alert("Sign up attempt {0} threw an exception: {1}.", this.uniqueID, ae.Message);
				throw new InternalErrorException();
			} catch (Exception e) {
				SetInternalErrorMsg();
				Log.Alert(e, "Sign up attempt {0} threw an exception.", this.uniqueID);
				throw new InternalErrorException();
			} // try
		} // CreateSecurityUserStuff

		private void CreateCustomerStuff() {
			Log.Debug("Sign up attempt '{0}': validating mobile phone...", this.uniqueID);

			bool mobilePhoneVerified = false;

			if (!this.model.CaptchaMode) {
				var vmc = new ValidateMobileCode(this.model.MobilePhone, this.model.MobileVerificationCode) {
					Transaction = this.dbTransaction
				};
				vmc.Execute();
				mobilePhoneVerified = vmc.IsValidatedSuccessfully();
			} // if

			Log.Debug("Sign up attempt '{0}': creating a customer entry...", this.uniqueID);

			var customer = new CreateCustomer(this, mobilePhoneVerified);
			customer.ExecuteNonQuery(this.dbTransaction);
			RefNumber = customer.RefNumber;

			Log.Debug("Sign up attempt '{0}': creating a campaign source reference entry...", this.uniqueID);

			if (!this.model.BrokerFillsForCustomer)
				new CreateCampaignSourceRef(this).ExecuteNonQuery(this.dbTransaction);

			Log.Debug("Sign up attempt '{0}': creating a requested loan entry...", this.uniqueID);

			new CreateCustomerRequestedLoan(this).ExecuteNonQuery(this.dbTransaction);

			Log.Debug("Sign up attempt '{0}': creating a source reference history entry...", this.uniqueID);

			SaveSourceRefHistory();

			if (this.model.IsAlibaba()) {
				Log.Debug("Sign up attempt '{0}': creating an Alibaba buyer entry...", this.uniqueID);
				new CreateAlibabaBuyer(this).ExecuteNonQuery(this.dbTransaction);
			} // if

			Log.Debug("Sign up attempt '{0}': customer stuff was created.", this.uniqueID);
		} // CreateCustomerStuff

		private void SaveSourceRefHistory() {
			Log.Debug("Saving sourceref history for user {0}...", UserID);

			var lst = new List<SourceRefEntry>();

			string[] arySourceRefs = (HttpUtility.UrlDecode(this.model.ReferenceSource) ?? string.Empty).Split(';');
			string[] aryVisitTimes = (HttpUtility.UrlDecode(this.model.VisitTimes) ?? string.Empty).Split(';');

			for (int i = 0; i < arySourceRefs.Length; i++) {
				string sSourceRef = arySourceRefs[i].Trim();

				if (string.IsNullOrWhiteSpace(sSourceRef))
					continue;

				DateTime? oVisitTime = null;

				if (i < aryVisitTimes.Length) {
					DateTime oTime;

					bool isParsed = DateTime.TryParseExact(
						aryVisitTimes[i],
						"dd/MM/yyyy HH:mm:ss",
						CultureInfo.InvariantCulture,
						DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
						out oTime
					);

					if (isParsed)
						oVisitTime = oTime;
				} // if

				lst.Add(new SourceRefEntry { SourceRef = sSourceRef, VisitTime = oVisitTime, });

				Log.Debug(
					"Sourceref entry for customer {0}: '{1}' on {2}.",
					UserID,
					sSourceRef,
					oVisitTime.HasValue
						? oVisitTime.Value.ToString("MMM d yyyy H:mm:ss", CultureInfo.InvariantCulture)
						: "unknown"
				);
			} // for each sourceref

			if (lst.Count > 0) {
				Log.Msg(
					"{1} entr{2} in sourceref history for user {0} complete.",
					UserID,
					lst.Count,
					lst.Count == 1 ? "y" : "ies"
				);

				DB.ExecuteNonQuery(
					this.dbTransaction,
					"SaveSourceRefHistory",
					CommandSpecies.StoredProcedure,
					new QueryParameter("UserID", UserID),
					DB.CreateTableParameter<SourceRefEntry>("Lst", lst)
				);
			}
			else
				Log.Msg("No sourceref history to save for user {0}.", UserID);

			Log.Debug("Saving sourceref history for user {0} complete.", UserID);
		} // SaveSourceRefHistory

		private static string GenerateUniqueID() {
			string id = Guid.NewGuid().ToString("N");

			return string.Join("-",
				Enumerable.Range(0, id.Length / IdChunkSize).Select(i => id.Substring(i * IdChunkSize, IdChunkSize))
			);
		} // GenerateUniqueID

		private void SetInternalErrorMsg(string errorMessage = null) {
			ErrorMsg = string.IsNullOrWhiteSpace(errorMessage) ? string.Format(
				"Internal server error. Please call support, error code is: '{0}'.",
				this.uniqueID
			) : errorMessage.Trim();
		} // SetInternalErrorMsg

		private readonly string uniqueID;
		private readonly SignupCustomerMultiOriginModel model;
		private ConnectionWrapper dbTransaction;
		private const int IdChunkSize = 4;

		private class SourceRefEntry {
			public string SourceRef { [UsedImplicitly] get; set; }
			public DateTime? VisitTime { [UsedImplicitly] get; set; }
		} // class SourceRefEntry

		private class UserNotCreatedException : Exception {} // class UserNotCreatedException
		private class BadDataException : Exception {} // class BadDataException
		private class InternalErrorException : Exception {} // class InternalErrorException

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		private class CreateUserForCustomer : AStoredProcedure {
			public enum Errors {
				DuplicateUser         = -1,
				OriginNotFound        = -2,
				[Description("role 'Web' was not found in the database")]
				RoleNotFound          = -3,
				[Description("failed to create Security_User entry")]
				FailedToCreateUser    = -4,
				[Description("failed to attach user to security role")]
				FailedToAttachRole    = -5,
				[Description("failed to create CustomerSession entry")]
				FailedToCreateSession = -6,
				[Description("customer email is already known as an underwriter login")]
				ConflictsWithInternal = -7,
				[Description("customer email is already known as a broker email")]
				ConflictsWithBroker   = -8,
			} // enum Errors

			public CreateUserForCustomer(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				if (OriginID <= 0) {
					Log.Warn("Origin id is not positive.");
					return false;
				} // if

				if (string.IsNullOrWhiteSpace(Email)) {
					Log.Warn("Email is not specified.");
					return false;
				} // if

				if (string.IsNullOrEmpty(EzPassword)) {
					Log.Warn("Password is not specified.");
					return false;
				} // if

				if (string.IsNullOrWhiteSpace(Salt)) {
					Log.Warn("Salt is not specified.");
					return false;
				} // if

				if (string.IsNullOrWhiteSpace(CycleCount)) {
					Log.Warn("Cycle count is not specified.");
					return false;
				} // if

				if (SecurityQuestionID <= 0) {
					Log.Warn("Security question is not specified.");
					return false;
				} // if

				if (string.IsNullOrWhiteSpace(SecurityAnswer)) {
					Log.Warn("Security answer is not specified.");
					return false;
				} // if

				return true;
			} // HasValidParameters

			[Length(255)]
			public string Email {
				get {
					if (string.IsNullOrWhiteSpace(this.email))
						return string.Empty;

					return this.email.Trim();
				} // get

				set {
					this.email = (value ?? string.Empty).Trim();

					if (string.IsNullOrWhiteSpace(this.email))
						this.email = string.Empty;
				} // set
			} // Email

			public int OriginID { get; set; }

			[Length(255)]
			public string EzPassword { get; set; }

			[Length(255)]
			public string Salt { get; set; }

			[Length(255)]
			public string CycleCount { get; set; }

			public int? SecurityQuestionID { get; set; }

			[Length(200)]
			public string SecurityAnswer { get; set; }

			[Length(50)]
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
			private string email;
		} // class CreateUserForCustomer

		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class CreateCustomer : AStoredProcedure {
			public CreateCustomer(SignupCustomerMultiOrigin stra, bool mobilePhoneVerified) : base(stra.DB, stra.Log) {
				this.stra = stra;
				this.refNumber = new RefNumber(this.stra.UserID);
				this.mobilePhoneVerified = mobilePhoneVerified;
			} // constructor

			public override bool HasValidParameters() {
				return (CustomerID > 0) && (OriginID > 0) && !string.IsNullOrWhiteSpace(UserName);
			} // HasValidParameters

			public int CustomerID {
				get { return this.stra.UserID; }
				set { }
			} // CustomerID

			[Length(128)]
			public string UserName {
				get { return this.stra.model.UserName; }
				set { }
			} // Name

			public int OriginID {
				// ReSharper disable once PossibleInvalidOperationException
				// If Origin is null this class won't even be created.
				get { return (int)this.stra.model.Origin.Value; }
				set { }
			} // OriginID

			[Length(250)]
			public string Status {
				get { return EZBob.DatabaseLib.Model.Database.Status.Registered.ToString(); }
				set { }
			} // Status

			[Length(8)]
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

			public bool? IsTest {
				get { return this.stra.model.IsTest; }
				set { }
			} // IsTest

			public int WhiteLabelID {
				get { return this.stra.model.WhiteLabelID; }
				set { }
			} // WhiteLabelID

			[Length(50)]
			public string MobilePhone {
				get { return this.stra.model.MobilePhone; }
				set { }
			} // MobilePhone

			public bool MobilePhoneVerified {
				get { return this.mobilePhoneVerified; }
				set { }
			} // MobilePhoneVerified

			[Length(250)]
			public string FirstName {
				get { return NormalizeName(this.stra.model.FirstName); }
				set { }
			} // FirstName

			[Length(250)]
			public string LastName {
				get { return NormalizeName(this.stra.model.LastName); }
				set { }
			} // LastName

			public int TrustPilotStatusID {
				get { return (int)EZBob.DatabaseLib.Model.Database.TrustPilotStauses.Neither; }
				set { }
			} // TrustPilotStatusID

			public DateTime GreetingMailSentDate {
				get { return DateTime.UtcNow; }
				set { }
			} // GreetingMailSentDate

			[Length(512)]
			public string ABTesting {
				get { return this.stra.model.ABTesting; }
				set { }
			} // ABTesting

			[Length(64)]
			public string FirstVisitTime {
				get { return this.stra.model.FirstVisitTime; }
				set { }
			} // FirstVisitTime

			[Length(1000)]
			public string ReferenceSource {
				get { return this.stra.model.BrokerFillsForCustomer ? "Broker" : this.stra.model.ReferenceSource; }
				set { }
			} // ReferenceSource

			[Length(300)]
			public string GoogleCookie {
				get { return this.stra.model.BrokerFillsForCustomer ? string.Empty : this.stra.model.GoogleCookie; }
				set { }
			} // GoogleCookie

			[Length(300)]
			public string AlibabaID {
				get { return this.stra.model.BrokerFillsForCustomer ? null : this.stra.model.AlibabaID; }
				set { }
			} // AlibabaID

			public bool? IsAlibaba {
				get {
					return !this.stra.model.BrokerFillsForCustomer && !string.IsNullOrWhiteSpace(this.stra.model.AlibabaID);
				}
				set { }
			} // IsAlibaba

			private static string NormalizeName(string name) {
				var s = (name ?? string.Empty).Trim();
				return s == string.Empty ? null : s;
			} // NormalizeName

			private readonly bool mobilePhoneVerified;
			private readonly SignupCustomerMultiOrigin stra;
			private readonly string refNumber;
		} // class CreateCustomer

		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class CreateCampaignSourceRef : AStoredProcedure {
			public CreateCampaignSourceRef(SignupCustomerMultiOrigin stra) : base(stra.DB, stra.Log) {
				this.stra = stra;
			} // constructor

			public override bool HasValidParameters() {
				return (CustomerID > 0);
			} // HasValidParameters

			public int CustomerID { get { return this.stra.UserID; } set { } }

			[Length(255)]
			public string FUrl { get { return this.stra.model.FUrl(); } set { } }
			[Length(255)]
			public string FSource { get { return this.stra.model.FSource(); } set { } }
			[Length(255)]
			public string FMedium { get { return this.stra.model.FMedium(); } set { } }
			[Length(255)]
			public string FTerm { get { return this.stra.model.FTerm(); } set { } }
			[Length(255)]
			public string FContent { get { return this.stra.model.FContent(); } set { } }
			[Length(255)]
			public string FName { get { return this.stra.model.FName(); } set { } }

			public DateTime? FDate { get { return this.stra.model.FDate(); } set { } }

			[Length(255)]
			public string RUrl { get { return this.stra.model.RUrl(); } set { } }
			[Length(255)]
			public string RSource { get { return this.stra.model.RSource() ?? "Direct"; } set { } }
			[Length(255)]
			public string RMedium { get { return this.stra.model.RMedium(); } set { } }
			[Length(255)]
			public string RTerm { get { return this.stra.model.RTerm(); } set { } }
			[Length(255)]
			public string RContent { get { return this.stra.model.RContent(); } set { } }
			[Length(255)]
			public string RName { get { return this.stra.model.RName(); } set { } }

			public DateTime RDate { get { return this.stra.model.RDate() ?? DateTime.UtcNow; } set { } }

			private readonly SignupCustomerMultiOrigin stra;
		} // class CreateCampaignSourceRef

		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class CreateCustomerRequestedLoan : AStoredProcedure {
			public CreateCustomerRequestedLoan(SignupCustomerMultiOrigin stra) : base(stra.DB, stra.Log) {
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

			private readonly SignupCustomerMultiOrigin stra;
		} // class CreateCustomerRequestedLoan

		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		private class CreateAlibabaBuyer : AStoredProcedure {
			public CreateAlibabaBuyer(SignupCustomerMultiOrigin stra) : base(stra.DB, stra.Log) {
				this.stra = stra;
			} // constructor

			public override bool HasValidParameters() {
				return (CustomerID > 0);
			} // HasValidParameters

			public int CustomerID { get { return this.stra.UserID; } set { } }

			public long AliID {
				get { return Convert.ToInt64(this.stra.model.AlibabaID); }
				set { }
			} // AliID

			private readonly SignupCustomerMultiOrigin stra;
		} // class CreateAlibabaBuyer
	} // class SignupAlibabaBuyer
} // namespace
