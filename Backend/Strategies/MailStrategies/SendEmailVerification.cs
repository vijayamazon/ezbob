namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class SendEmailVerification : AMailStrategyBase {
		#region constructor

		public SendEmailVerification(int customerId, string email, string address, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
			this.address = address;
			this.email = email;
		} // constructor

		#endregion constructor

		public override string Name { get { return "SendEmailVerification"; } } // Name

		#region method SetTemplateAndSubjectAndVariables

		protected override void SetTemplateAndSubjectAndVariables() {
			Subject = "Please verify your email";
			TemplateName = "Mandrill - Confirm your email";

			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName},
					{"Email", email},
					{"ConfirmEmailAddress", address}
				};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables

		private readonly string address;
		private readonly string email;
	} // class SendEmailVerification
} // namespace
