using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class PasswordRestored : AMailStrategyBase {
		#region constructor

		public PasswordRestored(int customerId, string password, AConnection oDB, ASafeLog oLog) : base(customerId, true, oDB, oLog) {
			this.password = password;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Password Restored"; } } // Name

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = "RestorePassword";
			TemplateName = "Mandrill - EZBOB password was restored";

			Variables = new Dictionary<string, string> {
				{"ProfilePage", "https://app.ezbob.com/Account/LogOn"},
				{"Password", password},
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables

		private readonly string password;
	} // class PasswordRestored
} // namespace
