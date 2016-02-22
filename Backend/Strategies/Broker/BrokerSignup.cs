namespace Ezbob.Backend.Strategies.Broker {
	using System;
	using ConfigManager;
	using Exceptions;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;
	using Misc;

	public class BrokerSignup : AStrategy {
		public BrokerSignup(
			string sFirmName,
			string sFirmRegNum,
			string sContactName,
			string sContactEmail,
			string sContactMobile,
			string sMobileCode,
			string sContactOtherPhone,
			DasKennwort password,
			DasKennwort passwordAgain,
			string sFirmWebSiteUrl,
			bool bIsCaptchaEnabled,
			int nBrokerTermsID,
			string sReferredBy,
			bool bFCARegistered,
			string sLicenseNumber,
			int uiOriginID
		) {
			this.isCaptchaEnabled = bIsCaptchaEnabled;
			this.mobileCode = sMobileCode;

			this.sp = new SpBrokerSignUp(password.Decrypt(), passwordAgain.Decrypt(), DB, Log) {
				FirmName = sFirmName,
				FirmRegNum = sFirmRegNum,
				ContactName = sContactName,
				ContactEmail = (sContactEmail ?? string.Empty).Trim().ToLowerInvariant(),
				ContactMobile = sContactMobile,
				ContactOtherPhone = sContactOtherPhone,
				EstimatedMonthlyClientAmount = 0,
				FirmWebSiteUrl = sFirmWebSiteUrl,
				EstimatedMonthlyApplicationCount = 0,
				BrokerTermsID = nBrokerTermsID,
				ReferredBy = sReferredBy,
				Strategy = this,
				FCARegistered = bFCARegistered,
				LicenseNumber = sLicenseNumber,
				UiOriginID = uiOriginID,
			};

			Properties = new BrokerProperties();
		} // constructor

		public override string Name { get { return "Broker signup"; } } // Name

		public BrokerProperties Properties { get; private set; }

		public override void Execute() {
			if (!this.isCaptchaEnabled) {
				var oValidator = new ValidateMobileCode(this.sp.ContactMobile, this.mobileCode);
				oValidator.Execute();

				if (!oValidator.IsValidatedSuccessfully())
					Properties.ErrorMsg = "Failed to validate mobile code.";
			} // if

			this.sp.FillFirst(Properties);

			if (!string.IsNullOrWhiteSpace(Properties.ErrorMsg)) {
				Log.Warn("Failed to create a broker: {0}", Properties.ErrorMsg);
				return;
			} // if

			if (Properties.BrokerID < 1) {
				Log.Alert("Failed to create a broker: no error message from DB but broker id is 0.");
				throw new StrategyException(this, "Failed to create a broker.");
			} // if
		} // Execute

		private readonly bool isCaptchaEnabled;
		private readonly string mobileCode;

		private readonly SpBrokerSignUp sp;

		private class SpBrokerSignUp : AStoredProc {
			public SpBrokerSignUp(
				string rawPassword,
				string rawPasswordAgain,
				AConnection oDB,
				ASafeLog oLog
			) : base(oDB, oLog) {
				this.rawPassword = rawPassword;
				this.rawPasswordAgain = rawPasswordAgain;
				SetPassword();
			} // constructor

			public override bool HasValidParameters() {
				FirmName = Validate(FirmName, "Broker name");
				FirmRegNum = Validate(FirmRegNum, "Broker registration number", false);
				ContactName = Validate(ContactName, "Contact person full name");
				ContactEmail = Validate(ContactEmail, "Contact person email").ToLowerInvariant();

				ContactOtherPhone = Validate(ContactOtherPhone, "Contact person other phone", false);
				
				this.rawPassword = Validate(this.rawPassword, "Password");

				if (this.rawPassword != this.rawPasswordAgain)
					throw new StrategyWarning(Strategy, "Passwords do not match.");

				SetPassword();

				FirmWebSiteUrl = (FirmWebSiteUrl ?? string.Empty).Trim();

				return true;
			} // HasValidParameters

			[UsedImplicitly]
			public string FirmName { get; set; }

			[UsedImplicitly]
			public string FirmRegNum { get; set; }

			[UsedImplicitly]
			public string ContactName { get; set; }

			[UsedImplicitly]
			public string ContactEmail {
				get { return this.contactEmail; }
				set {
					this.contactEmail = value;
					SetPassword();
				} // set
			} // ContactEmail

			[UsedImplicitly]
			public string ContactMobile { get; set; }

			[UsedImplicitly]
			public string ContactOtherPhone { get; set; }

			[UsedImplicitly]
			public decimal EstimatedMonthlyClientAmount { get; set; }

			[UsedImplicitly]
			public string Password { get; set; } // Password

			[UsedImplicitly]
			public string Salt { get; set; } // Salt

			[UsedImplicitly]
			public string CycleCount { get; set; } // CycleCount

			private void SetPassword() {
				var pu = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

				var pass = pu.Generate(ContactEmail, this.rawPassword);

				Password = pass.Password;
				CycleCount = pass.CycleCount;
				Salt = pass.Salt;
			} // SetPassword

			[UsedImplicitly]
			public string FirmWebSiteUrl { get; set; }

			[UsedImplicitly]
			public int EstimatedMonthlyApplicationCount { get; set; }

			// ReSharper disable ValueParameterNotUsed

			[UsedImplicitly]
			public string TempSourceRef {
				get { return Guid.NewGuid().ToString("N"); }
				set { }
			} // TempSourceRef

			[UsedImplicitly]
			public DateTime AgreedToTermsDate {
				get { return DateTime.UtcNow; }
				set { }
			} // AgreedToTermsDate

			[UsedImplicitly]
			public DateTime AgreedToPrivacyPolicyDate {
				get { return DateTime.UtcNow; }
				set { }
			} // AgreedToPrivacyPolicyDate

			// ReSharper restore ValueParameterNotUsed

			[UsedImplicitly]
			public int BrokerTermsID { get; set; }

			[UsedImplicitly]
			public string ReferredBy { get; set; }

			[UsedImplicitly]
			public bool FCARegistered { get; set; }

			[UsedImplicitly]
			public string LicenseNumber { get; set; }

			[UsedImplicitly]
			public int UiOriginID { get; set; }

			[NonTraversable]
			public AStrategy Strategy { private get; set; }

			private string Validate(string sValue, string sArgName, bool bThrow = true) {
				sValue = (sValue ?? string.Empty).Trim();

				if (sValue.Length == 0) {
					if (bThrow)
						throw new StrategyWarning(Strategy, sArgName + " not specified.");

					return sValue;
				} // if

				if (sValue.Length > 255) {
					if (bThrow)
						throw new StrategyWarning(Strategy, sArgName + " is too long.");

					return sValue.Substring(0, 255);
				} // if

				return sValue;
			} // Validate

			private string contactEmail;
			private string rawPassword;
			private readonly string rawPasswordAgain;
		} // SpBrokerSignUp
	} // class BrokerSignup
} // namespace Ezbob.Backend.Strategies.Broker
