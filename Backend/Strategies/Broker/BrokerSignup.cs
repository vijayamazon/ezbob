namespace EzBob.Backend.Strategies.Broker {
	using System;
	using System.Text;
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
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_oCreateSp = new SpBrokerSignUp(DB, Log) {
				FirmName = sFirmName,
				FirmRegNum = sFirmRegNum,
				ContactName = sContactName,
				ContactEmail = sContactEmail,
				ContactMobile = sContactMobile,
				MobileCode = sMobileCode,
				ContactOtherPhone = sContactOtherPhone,
				EstimatedMonthlyClientAmount = nEstimatedMonthlyClientAmount,
				Password = sPassword,
				Password2 = sPassword2,
				FirmWebSiteUrl = sFirmWebSiteUrl,
				EstimatedMonthlyApplicationCount = nEstimatedMonthlyApplicationCount,
			};

			m_oSetSp = new SpBrokerSetSourceRef(DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Broker signup"; } } // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			string sErrMsg = null;
			int nBrokerID = 0;

			m_oCreateSp.ForEachRowSafe((sr, bRowsetStart) => {
				sr.Read
					.To(out sErrMsg)
					.To(out nBrokerID);

				return ActionResult.SkipAll;
			});

			if (!string.IsNullOrWhiteSpace(sErrMsg)) {
				Log.Alert("Failed to create a broker: {0}", sErrMsg);
				throw new Exception(sErrMsg);
			} // if

			if (nBrokerID < 1) {
				Log.Alert("Failed to create a broker: no error message from DB but broker id is 0.");
				throw new Exception("Failed to create a broker.");
			} // if

			string sBrokerID = BaseConverter.Execute(nBrokerID, BaseConverter.LowerCaseLetters);

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

			m_oSetSp.BrokerID = nBrokerID;
			m_oSetSp.SourceRef = oSrcRef.ToString();
			m_oSetSp.ExecuteNonQuery();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

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

				var oValidator = new ValidateMobileCode(ContactMobile, MobileCode, DB, Log);
				oValidator.Execute();
				if (!oValidator.IsValidatedSuccessfully())
					throw new Exception("Failed to validate mobile code.");

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

			[Traversable]
			public string FirmName { get; set; }

			[Traversable]
			public string FirmRegNum { get; set; }

			[Traversable]
			public string ContactName { get; set; }

			[Traversable]
			public string ContactEmail { get; set; }

			[Traversable]
			public string ContactMobile { get; set; }

			public string MobileCode { get; set; }

			[Traversable]
			public string ContactOtherPhone { get; set; }

			[Traversable]
			public decimal EstimatedMonthlyClientAmount { get; set; }

			[Traversable]
			public string Password {
				get { return SecurityUtils.HashPassword(m_sPassword); } // get
				set { m_sPassword = value; } // set
			} // Password

			private string m_sPassword;

			[Traversable]
			public string TempSourceRef {
				get { return Guid.NewGuid().ToString("N"); }
				set { }
			} // TempSourceRef

			[Traversable]
			public string FirmWebSiteUrl { get; set; }

			[Traversable]
			public int EstimatedMonthlyApplicationCount { get; set; }

			[Traversable]
			public DateTime AgreedToTermsDate {
				get { return DateTime.UtcNow; }
				set { }
			} // AgreedToTermsDate

			[Traversable]
			public DateTime AgreedToPrivacyPolicyDate {
				get { return DateTime.UtcNow; }
				set { }
			} // AgreedToPrivacyPolicyDate

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
