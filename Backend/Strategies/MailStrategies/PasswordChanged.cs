namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using UserManagement.EmailConfirmation;

	public class PasswordChanged : AMailStrategyBase {
		public PasswordChanged(int customerId, DasKennwort oPassword) : base(customerId, true) {
			this.rawPassword = oPassword.Decrypt();
		} // constructor

		internal PasswordChanged(int customerId, string sPassword) : base(customerId, true) {
			this.rawPassword = sPassword;
		} // constructor

		public override string Name { get { return "Password Changed"; } } // Name

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - New password";

			Variables = new Dictionary<string, string> {
				{ "Password", this.rawPassword },
				{ "FirstName", FirstName }
			};
		} // SetTemplateAndVariables

		protected virtual string FirstName {
			get { return CustomerData.FirstName; } // get
		} // FirstName

		private readonly string rawPassword;
	} // class PasswordChanged
} // namespace
