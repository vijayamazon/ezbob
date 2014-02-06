namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class PasswordChanged : AMailStrategyBase {
		#region constructor

		public PasswordChanged(int customerId, string password, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
			this.password = password;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Password Changed"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - New password";

			Variables = new Dictionary<string, string> {
				{"Password", password},
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		private readonly string password;
	} // class PasswordChanged
} // namespace
