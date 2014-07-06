namespace EzBob.Backend.Strategies.Broker {
	using System;
	using Exceptions;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;
	using Misc;

	public class BrokerSignup : AStrategy {
		#region public

		#region constructor

		public BrokerSignup(
			string sFirmName,
			string sFirmRegNum,
			string sContactName,
			string sContactEmail,
			string sContactMobile,
			string sMobileCode,
			string sContactOtherPhone,
			decimal nEstimatedMonthlyClientAmount,
			Password oPassword,
			string sFirmWebSiteUrl,
			int nEstimatedMonthlyApplicationCount,
			bool bIsCaptchaEnabled,
			int nBrokerTermsID,
			string sReferredBy,
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_bIsCaptchaEnabled = bIsCaptchaEnabled;
			m_sMobileCode = sMobileCode;

			m_oCreateSp = new SpBrokerSignUp(DB, Log) {
				FirmName = sFirmName,
				FirmRegNum = sFirmRegNum,
				ContactName = sContactName,
				ContactEmail = sContactEmail,
				ContactMobile = sContactMobile,
				ContactOtherPhone = sContactOtherPhone,
				EstimatedMonthlyClientAmount = nEstimatedMonthlyClientAmount,
				Password = oPassword.Primary,
				Password2 = oPassword.Confirmation,
				FirmWebSiteUrl = sFirmWebSiteUrl,
				EstimatedMonthlyApplicationCount = nEstimatedMonthlyApplicationCount,
				BrokerTermsID = nBrokerTermsID,
				ReferredBy = sReferredBy,
				Strategy = this,
			};

			Properties = new BrokerProperties();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Broker signup"; } } // Name

		#endregion property Name

		#region property Properties

		public BrokerProperties Properties { get; private set; }

		#endregion property Properties

		#region method Execute

		public override void Execute() {
			if (!m_bIsCaptchaEnabled) {
				var oValidator = new ValidateMobileCode(m_oCreateSp.ContactMobile, m_sMobileCode, DB, Log);
				oValidator.Execute();
				if (!oValidator.IsValidatedSuccessfully())
				{
					Properties.ErrorMsg = "Failed to validate mobile code.";
				}
			} // if

			m_oCreateSp.FillFirst(Properties);

			if (!string.IsNullOrWhiteSpace(Properties.ErrorMsg)) {
				Log.Warn("Failed to create a broker: {0}", Properties.ErrorMsg);
				return;
			} // if

			if (Properties.BrokerID < 1) {
				Log.Alert("Failed to create a broker: no error message from DB but broker id is 0.");
				throw new StrategyException(this, "Failed to create a broker.");
			} // if
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly bool m_bIsCaptchaEnabled;
		private readonly string m_sMobileCode;

		private readonly SpBrokerSignUp m_oCreateSp;

		#region class SpBrokerSignUp

		private class SpBrokerSignUp : AStoredProc {
			#region constructor

			public SpBrokerSignUp(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters() {
				FirmName = Validate(FirmName, "Broker name");
				FirmRegNum = Validate(FirmRegNum, "Broker registration number", false);
				ContactName = Validate(ContactName, "Contact person full name");
				ContactEmail = Validate(ContactEmail, "Contact person email");

				ContactOtherPhone = Validate(ContactOtherPhone, "Contact person other phone", false);

				if (EstimatedMonthlyClientAmount <= 0)
					throw new StrategyWarning(Strategy, "Estimated monthly amount is out of range.");

				Password = Validate(m_sPassword, "Password");

				if (m_sPassword != Password2)
					throw new StrategyWarning(Strategy, "Passwords do not match.");

				FirmWebSiteUrl = (FirmWebSiteUrl ?? string.Empty).Trim();

				return true;
			} // HasValidParameters

			#endregion method HasValidParameters

			#region properties

			[UsedImplicitly]
			public string FirmName { get; set; }

			[UsedImplicitly]
			public string FirmRegNum { get; set; }

			[UsedImplicitly]
			public string ContactName { get; set; }

			[UsedImplicitly]
			public string ContactEmail { get; set; }

			[UsedImplicitly]
			public string ContactMobile { get; set; }

			[UsedImplicitly]
			public string ContactOtherPhone { get; set; }

			[UsedImplicitly]
			public decimal EstimatedMonthlyClientAmount { get; set; }

			#region property Password

			[UsedImplicitly]
			public string Password {
				get { return SecurityUtils.HashPassword(ContactEmail, m_sPassword); } // get
				set { m_sPassword = value; } // set
			} // Password

			private string m_sPassword;

			#endregion property Password

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

			[NonTraversable]
			[UsedImplicitly]
			public string Password2 { get; set; }

			[UsedImplicitly]
			public int BrokerTermsID { get; set; }

			[UsedImplicitly]
			public string ReferredBy { get; set; }

			#endregion properties

			[NonTraversable]
			public AStrategy Strategy { private get; set; }

			#region method Validate

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

			#endregion method Validate
		} // SpBrokerSignUp

		#endregion class SpBrokerSignUp

		#endregion private
	} // class BrokerSignup
} // namespace EzBob.Backend.Strategies.Broker
