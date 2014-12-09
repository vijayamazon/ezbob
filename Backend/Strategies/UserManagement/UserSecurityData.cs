namespace Ezbob.Backend.Strategies.UserManagement {
	using System.Globalization;
	using System.Text.RegularExpressions;
	using Exceptions;

	internal sealed class UserSecurityData {

		public const string WebRole = "Web";

		public UserSecurityData(AStrategy oStrategy) {
			m_oStrategy = oStrategy;

			Cfg = new UserManagementConfigs(m_oStrategy.DB, m_oStrategy.Log);

			m_oStrategy.Log.Debug("User management configuration: {0}.", Cfg);
		} // constructor

		public string Email { get; set; }

		public void ValidateEmail(bool isUW = false) {
			string sEmail = Email.Trim().ToLower(CultureInfo.InvariantCulture);

			if (Cfg.Underwriters.Contains(sEmail))
				return;

			// Email is invalid if it arrived from some strange source.
			// Our UI does this validation...
            if (!isUW)
            {
                if (!Regex.IsMatch(sEmail, Cfg.LoginValidationStringForWeb))
                    throw new StrategyAlert(m_oStrategy, "Login does not conform to the password security policy.");

                if (!RegexValidate("Login", sEmail, Cfg.LoginValidity))
                    throw new StrategyAlert(m_oStrategy, "Can't validate login");
            }
		} // ValidateEmail

		public string OldPassword { get; set; }

		public string OldPasswordHash {
			get { return Ezbob.Utils.Security.SecurityUtils.HashPassword(Email, OldPassword); }
		} // OldPasswordHash

		public void ValidateOldPassword() {
			// Password is invalid if it arrived from some strange source.
			// Our UI does this validation...
			if (!RegexValidate("Password", OldPassword, Cfg.PasswordValidity))
				throw new StrategyAlert(m_oStrategy, "Can't validate password");
		} // ValidateOldPassword

		public string NewPassword { get; set; }

		public string NewPasswordHash {
			get { return Ezbob.Utils.Security.SecurityUtils.HashPassword(Email, NewPassword); }
		} // NewPasswordHash

		public void ValidateNewPassword() {
			// Password is invalid if it arrived from some strange source.
			// Our UI does this validation...
			if (!RegexValidate("Password", NewPassword, Cfg.PasswordValidity))
				throw new StrategyAlert(m_oStrategy, "Can't validate password");
		} // ValidateNewPassword

		public int PasswordQuestion { get; set; }

		public string PasswordAnswer { get; set; }

		public UserManagementConfigs Cfg { get; private set; }

		private readonly AStrategy m_oStrategy;

		private bool RegexValidate(string sValueName, string sValueToValidate, string sRegEx) {
			try {
				return Regex.IsMatch(sValueToValidate, sRegEx);
			}
			catch {
				m_oStrategy.Log.Warn("{2}: '{0}' doesn't match: '{1}'.", sValueToValidate, sRegEx, sValueName);
				return false;
			} // try
		} // RegexValidate

	} // class UserSecurityData
} // namespace Ezbob.Backend.Strategies.UserManagement
