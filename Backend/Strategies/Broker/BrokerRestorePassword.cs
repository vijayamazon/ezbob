namespace EzBob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;
	using MailStrategies;
	using Misc;

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

			var oPassword = new SimplePassword(8, oProperties.ContactEmail);

			DB.ExecuteNonQuery(
				"BrokerResetPassword",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@BrokerID", oProperties.BrokerID),
				new QueryParameter("@Password", oPassword.Hash)
			);

			new BrokerPasswordRestored(oProperties.BrokerID, oPassword.RawValue, DB, Log).Execute();
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
} // namespace EzBob.Backend.Strategies.Broker
