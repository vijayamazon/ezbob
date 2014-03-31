﻿namespace EzBob.Backend.Strategies.Broker {
	using System;
	using System.Text;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Security;

	#region class BrokerSignup

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
			string sPassword,
			string sPassword2,
			string sFirmWebSiteUrl,
			int nEstimatedMonthlyApplicationCount,
			bool bIsCaptchaEnabled,
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
				Password = sPassword,
				Password2 = sPassword2,
				FirmWebSiteUrl = sFirmWebSiteUrl,
				EstimatedMonthlyApplicationCount = nEstimatedMonthlyApplicationCount,
			};

			m_oSetSp = new SpBrokerSetSourceRef(DB, Log);

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
					throw new Exception("Failed to validate mobile code.");
			} // if

			m_oCreateSp.FillFirst(Properties);

			if (!string.IsNullOrWhiteSpace(Properties.ErrorMsg)) {
				Log.Alert("Failed to create a broker: {0}", Properties.ErrorMsg);
				throw new Exception(Properties.ErrorMsg);
			} // if

			if (Properties.BrokerID < 1) {
				Log.Alert("Failed to create a broker: no error message from DB but broker id is 0.");
				throw new Exception("Failed to create a broker.");
			} // if

			string sBrokerID = BaseConverter.Execute(Properties.BrokerID, BaseConverter.LowerCaseLetters);

			var oSrcRef = new StringBuilder("brk-");

			const int nMaxSrcRefLen = 6;

			if (sBrokerID.Length < nMaxSrcRefLen) {
				const int cMin = 'a';
				const int cMax = 1 + (int)'z';
				int nLen = nMaxSrcRefLen - sBrokerID.Length;

				var rnd = new Random();
				for (int i = 0; i < nLen; i++)
					oSrcRef.Append((char)rnd.Next(cMin, cMax));
			} // if

			oSrcRef.Append(sBrokerID);

			m_oSetSp.BrokerID = Properties.BrokerID;
			m_oSetSp.SourceRef = oSrcRef.ToString();
			m_oSetSp.ExecuteNonQuery();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly bool m_bIsCaptchaEnabled;
		private readonly string m_sMobileCode;

		private readonly SpBrokerSignUp m_oCreateSp;
		private readonly SpBrokerSetSourceRef m_oSetSp;

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
					throw new Exception("Estimated monthly amount is out of range.");

				Password = Validate(m_sPassword, "Password");

				if (m_sPassword != Password2)
					throw new Exception("Passwords do not match.");

				FirmWebSiteUrl = (FirmWebSiteUrl ?? string.Empty).Trim();

				return true;
			} // HasValidParameters

			#endregion method HasValidParameters

			#region properties

			public string FirmName { get; set; }

			public string FirmRegNum { get; set; }

			public string ContactName { get; set; }

			public string ContactEmail { get; set; }

			public string ContactMobile { get; set; }

			public string ContactOtherPhone { get; set; }

			public decimal EstimatedMonthlyClientAmount { get; set; }

			#region property Password

			public string Password {
				get { return SecurityUtils.HashPassword(ContactEmail + m_sPassword); } // get
				set { m_sPassword = value; } // set
			} // Password

			private string m_sPassword;

			#endregion property Password

			public string TempSourceRef {
				get { return Guid.NewGuid().ToString("N"); }
				set { }
			} // TempSourceRef

			public string FirmWebSiteUrl { get; set; }

			public int EstimatedMonthlyApplicationCount { get; set; }

			public DateTime AgreedToTermsDate {
				get { return DateTime.UtcNow; }
				set { }
			} // AgreedToTermsDate

			public DateTime AgreedToPrivacyPolicyDate {
				get { return DateTime.UtcNow; }
				set { }
			} // AgreedToPrivacyPolicyDate

			[NonTraversable]
			public string Password2 { get; set; }

			#endregion properties

			#region method Validate

			private string Validate(string sValue, string sArgName, bool bThrow = true) {
				sValue = (sValue ?? string.Empty).Trim();

				if (sValue.Length == 0) {
					if (bThrow)
						throw new ArgumentNullException(sArgName, sArgName + " not specified.");

					return sValue;
				} // if

				if (sValue.Length > 255) {
					if (bThrow)
						throw new Exception(sArgName + " is too long.");

					return sValue.Substring(0, 255);
				} // if

				return sValue;
			} // Validate

			#endregion method Validate
		} // SpBrokerSignUp

		#endregion class SpBrokerSignUp

		#region class SpBrokerSetSourceRef

		private class SpBrokerSetSourceRef : AStoredProc {
			#region constructor

			public SpBrokerSetSourceRef(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			#endregion constructor

			#region method HasValidParameters

			public override bool HasValidParameters() {
				return (BrokerID > 0) && !string.IsNullOrWhiteSpace(SourceRef);
			} // HasValidParameters

			#endregion method HasValidParameters

			#region properties

			public int BrokerID { get; set; }

			public string SourceRef {
				get { return m_sSourceRef; } // get
				set { m_sSourceRef = (value ?? string.Empty).Trim();

					if (m_sSourceRef.Length > 10)
						m_sSourceRef = m_sSourceRef.Substring(0, 10);
				} // set
			} // SourceRef

			private string m_sSourceRef;

			#endregion properties
		} // SpBrokerSetSourceRef

		#endregion class SpBrokerSetSourceRef

		#endregion private
	} // class BrokerSignup

	#endregion class BrokerSignup
} // namespace EzBob.Backend.Strategies.Broker
