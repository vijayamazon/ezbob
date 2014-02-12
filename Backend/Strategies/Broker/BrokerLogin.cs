namespace EzBob.Backend.Strategies.Broker {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Security;

	#region class BrokerLogin

	public class BrokerLogin : AStrategy {
		#region public

		#region constructor

		public BrokerLogin(string sEmail, string sPassword, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_sEmail = sEmail;
			m_sPassword = sPassword;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name { get { return "Broker login"; } } // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_sEmail = Validate(m_sEmail, "Email");
			m_sPassword = Validate(m_sPassword, "Password");

			string sErrMsg = DB.ExecuteScalar<string>(
				"BrokerLogin",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@Email", m_sEmail),
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

		private string m_sEmail;
		private string m_sPassword;

		#endregion private
	} // class BrokerLogin

	#endregion class BrokerLogin
} // namespace EzBob.Backend.Strategies.Broker
