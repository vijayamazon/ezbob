using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class PasswordChanged : AMailStrategyBase {
		#region constructor

		public PasswordChanged(int customerId, string password, AConnection oDB, ASafeLog oLog) : base(customerId, true, oDB, oLog) {
			this.password = password;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Password Changed"; } } // Name

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = "Your new ezbob password has been registered.";
			TemplateName = "Mandrill - New password";

			Variables = new Dictionary<string, string> {
				{"Password", password},
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables

		private readonly string password;
	} // class PasswordChanged
} // namespace
