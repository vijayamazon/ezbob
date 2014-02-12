namespace EzBob.Backend.Strategies.Broker {
	using System;
	using System.Text;
	using Ezbob.Database;
	using Ezbob.Logger;
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
			AConnection oDB,
			ASafeLog oLog
		) : base(oDB, oLog) {
			m_sFirmName = sFirmName;
			m_sFirmRegNum = sFirmRegNum;
			m_sContactName = sContactName;
			m_sContactEmail = sContactEmail;
			m_sContactMobile = sContactMobile;
			m_sMobileCode = sMobileCode;
			m_sContactOtherPhone = sContactOtherPhone;
			m_nEstimatedMonthlyClientAmount = nEstimatedMonthlyClientAmount;
			m_sPassword = sPassword;
			m_sPassword2 = sPassword2;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Broker signup"; } } // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_sFirmName = Validate(m_sFirmName, "Firm name");
			m_sFirmRegNum = Validate(m_sFirmRegNum, "Firm registration number", false);
			m_sContactName = Validate(m_sContactName, "Contact person name");
			m_sContactEmail = Validate(m_sContactEmail, "Contact person email");

			var oValidator = new ValidateMobileCode(m_sContactMobile, m_sMobileCode, DB, Log);
			oValidator.Execute();
			if (!oValidator.IsValidatedSuccessfully())
				throw new Exception("Failed to validate mobile code.");

			m_sContactOtherPhone = Validate(m_sContactOtherPhone, "Contact person other phone", false);

			if (m_nEstimatedMonthlyClientAmount <= 0)
				throw new Exception("Estimated monthly amount is out of range.");

			m_sPassword = Validate(m_sPassword, "Password");

			if (m_sPassword != m_sPassword2)
				throw new Exception("Passwords do not match.");

			var oSrcRef = new StringBuilder("brk-");

			const int cMin = 'a';
			const int cMax = 1 + (int)'z';

			var rnd = new Random();
			for (int i = 0; i < 6; i++)
				oSrcRef.Append((char)rnd.Next(cMin, cMax));

			string sErrMsg = DB.ExecuteScalar<string>(
				"BrokerSignUp",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@FirmName", m_sFirmName),
				new QueryParameter("@FirmRegNum", m_sFirmRegNum),
				new QueryParameter("@ContactName", m_sContactName),
				new QueryParameter("@ContactEmail", m_sContactEmail),
				new QueryParameter("@ContactMobile", m_sContactMobile),
				new QueryParameter("@ContactOtherPhone", m_sContactOtherPhone),
				new QueryParameter("@SourceRef", oSrcRef.ToString()),
				new QueryParameter("@EstimatedMonthlyClientAmount", m_nEstimatedMonthlyClientAmount),
				new QueryParameter("@Password", SecurityUtils.HashPassword(m_sPassword))
			);

			if (!string.IsNullOrWhiteSpace(sErrMsg))
				throw new Exception(sErrMsg);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

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

		private string m_sFirmName;
		private string m_sFirmRegNum;
		private string m_sContactName;
		private string m_sContactEmail;
		private string m_sContactMobile;
		private string m_sMobileCode;
		private string m_sContactOtherPhone;
		private decimal m_nEstimatedMonthlyClientAmount;
		private string m_sPassword;
		private string m_sPassword2;

		#endregion private
	} // class BrokerSignup

	#endregion class BrokerSignup
} // namespace EzBob.Backend.Strategies.Broker
