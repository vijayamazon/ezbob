namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ThreeInvalidAttempts : AMailStrategyBase {
		#region constructor

		public ThreeInvalidAttempts(int customerId, string password, AConnection oDb, ASafeLog oLog)
			: base(customerId, true, oDb, oLog) {
			this.password = password;
		} // constructor

		#endregion constructor

		public override string Name { get {return "Three Invalid Attempts"; } } // Name

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = "Three unsuccessful login attempts to your account have been made.";
			TemplateName = "Mandrill - Temporary password";

			Variables = new Dictionary<string, string> {
				{"Password", password},
				{"FirstName", string.IsNullOrEmpty(CustomerData.FirstName) ? "customer" : CustomerData.FirstName},
				{"ProfilePage", "https://app.ezbob.com/Customer/Profile"},
				{"NIMRODTELEPHONENUMBER", "+44 800 011 4787"} // TODO: change name of variable here and in mandrill\mailchimp
			};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables

		private readonly string password;
	} // class ThreeInvalidAttempts
} // namespace
