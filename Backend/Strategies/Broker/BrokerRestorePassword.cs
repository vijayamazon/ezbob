namespace EzBob.Backend.Strategies.Broker {
	using System;
	using System.Text;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using MailStrategies;

	#region class BrokerRestorePassword

	public class BrokerRestorePassword : AStrategy {
		#region public

		#region constructor

		public BrokerRestorePassword(string sMobile, string sCode, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_sMobile = sMobile;
			m_sCode = sCode;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Broker restore password"; } } // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			var oValidator = new ValidateMobileCode(m_sMobile, m_sCode, DB, Log);
			oValidator.Execute();
			if (!oValidator.IsValidatedSuccessfully())
				throw new Exception("Failed to validate mobile code.");

			var sp = new SpBrokerLoadOwnProperties(DB, Log) {
				ContactMobile = m_sMobile,
			};

			BrokerProperties oProperties = sp.FillFirst<BrokerProperties>();

			Log.Debug("Broker properties search result for mobile phone {0}:\n{1}", m_sMobile, oProperties);

			var osPassword = new StringBuilder();

			const int cLowerMin = 'a';
			const int cLowerMax = 1 + (int)'z';

			const int cUpperMin = 'A';
			const int cUpperMax = 1 + (int)'Z';

			const int cDigitMin = '0';
			const int cDigitMax = 1 + (int)'9';

			var rnd = new Random();

			for (int i = 1; i <= 8; i++) {
				if (i % 3 == 0)
					osPassword.Append((char)rnd.Next(cDigitMin, cDigitMax));
				else if (i % 2 == 0)
					osPassword.Append((char)rnd.Next(cUpperMin, cUpperMax));
				else
					osPassword.Append((char)rnd.Next(cLowerMin, cLowerMax));
			} // for

			DB.ExecuteNonQuery(
				"BrokerResetPassword",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@BrokerID", oProperties.BrokerID),
				new QueryParameter("@Password", SecurityUtils.HashPassword(oProperties.ContactEmail + osPassword.ToString()))
			);

			new BrokerPasswordRestored(oProperties.BrokerID, osPassword.ToString(), DB, Log).Execute();
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

		private readonly string m_sMobile;
		private readonly string m_sCode;

		#endregion private
	} // class BrokerRestorePassword

	#endregion class BrokerRestorePassword
} // namespace EzBob.Backend.Strategies.Broker
