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

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - EZBOB password was restored";

			Variables = new Dictionary<string, string> {
				{"ProfilePage", ProfilePage},
				{"Password", password},
				{"FirstName", string.IsNullOrWhiteSpace(CustomerData.FirstName) ? Salutation : CustomerData.FirstName }
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		#region property ProfilePage

		protected virtual string ProfilePage {
			get { return "https://app.ezbob.com/Account/LogOn"; }
		} // ProfilePage

		#endregion property ProfilePage

		#region property Salutation

		protected virtual string Salutation {
			get { return "Dear customer"; }
		} // Salutation

		#endregion property Salutation

		private readonly string password;
	} // class PasswordRestored
} // namespace
