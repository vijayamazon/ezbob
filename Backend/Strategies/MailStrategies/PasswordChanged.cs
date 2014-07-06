namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class PasswordChanged : AMailStrategyBase {
		#region constructor

		public PasswordChanged(int customerId, Password oPassword, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
			m_sPassword = oPassword.Primary;
		} // constructor

		internal PasswordChanged(int customerId, string sPassword, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
			m_sPassword = sPassword;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Password Changed"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - New password";

			Variables = new Dictionary<string, string> {
				{"Password", m_sPassword},
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		private readonly string m_sPassword;
	} // class PasswordChanged
} // namespace
