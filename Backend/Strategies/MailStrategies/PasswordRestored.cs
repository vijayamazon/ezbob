namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class PasswordRestored : AMailStrategyBase {
		#region constructor

		public PasswordRestored(int customerId, string password, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
			this.password = password;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Password Restored"; } } // Name

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = "Your ezbob password has been restored";
			TemplateName = "Mandrill - EZBOB password was restored";

			Variables = new Dictionary<string, string> {
				{"ProfilePage", "https://app.ezbob.com/Account/LogOn"},
				{"Password", password},
				{"FirstName", string.IsNullOrEmpty(CustomerData.FirstName) ? "Dear customer" : CustomerData.FirstName }
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables

		private readonly string password;
	} // class PasswordRestored
} // namespace
