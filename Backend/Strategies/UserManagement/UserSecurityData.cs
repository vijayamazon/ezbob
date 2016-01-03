namespace Ezbob.Backend.Strategies.UserManagement {
	using System.Globalization;
	using System.Text.RegularExpressions;
	using Exceptions;

	internal sealed class UserSecurityData {
		public const string WebRole = "Web";

		public UserSecurityData(AStrategy oStrategy) {
			this.strategy = oStrategy;

			Cfg = new UserManagementConfigs(this.strategy.DB, this.strategy.Log);

			this.strategy.Log.Debug("User management configuration: {0}.", Cfg);
		} // constructor

		public string Email { get; set; }

		public void ValidateEmail(bool isUW = false) {
			string sEmail = Email.Trim().ToLower(CultureInfo.InvariantCulture);

			if (Cfg.Underwriters.Contains(sEmail))
				return;

			// Email is invalid if it arrived from some strange source.
			// Our UI does this validation...
			if (!isUW) {
				if (!Regex.IsMatch(sEmail, Cfg.LoginValidationStringForWeb))
					throw new StrategyAlert(this.strategy, "Login does not conform to the password security policy.");

				if (!RegexValidate("Login", sEmail, Cfg.LoginValidity))
					throw new StrategyAlert(this.strategy, "Can't validate login");
			} // if
		} // ValidateEmail

		public string OldPassword { get; set; }

		public void ValidateOldPassword() {
			// Password is invalid if it arrived from some strange source.
			// Our UI does this validation...
			if (!RegexValidate("Password", OldPassword, Cfg.PasswordValidity))
				throw new StrategyAlert(this.strategy, "Can't validate password");
		} // ValidateOldPassword

		public string NewPassword { get; set; }

		public void ValidateNewPassword() {
			// Password is invalid if it arrived from some strange source.
			// Our UI does this validation...
			if (!RegexValidate("Password", NewPassword, Cfg.PasswordValidity))
				throw new StrategyAlert(this.strategy, "Can't validate password");
		} // ValidateNewPassword

		public int PasswordQuestion { get; set; }

		public string PasswordAnswer { get; set; }

		public UserManagementConfigs Cfg { get; private set; }

		private readonly AStrategy strategy;

		private bool RegexValidate(string sValueName, string sValueToValidate, string sRegEx) {
			try {
				return Regex.IsMatch(sValueToValidate, sRegEx);
			} catch {
				this.strategy.Log.Warn("{2}: '{0}' doesn't match: '{1}'.", sValueToValidate, sRegEx, sValueName);
				return false;
			} // try
		} // RegexValidate
	} // class UserSecurityData
} // namespace Ezbob.Backend.Strategies.UserManagement
