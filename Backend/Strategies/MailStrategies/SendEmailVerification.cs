using System.Collections.Generic;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies.MailStrategies {
	public class SendEmailVerification : AMailStrategyBase {
		#region constructor

		public SendEmailVerification(int customerId, string address, AConnection oDB, ASafeLog oLog) : base(customerId, true, oDB, oLog) {
			this.address = address;
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
					{"Email", CustomerData.Mail},
					{"ConfirmEmailAddress", address}
				};
		} // SetTemplateAndSubjectAndVariables

		#endregion method SetTemplateAndSubjectAndVariables

		private readonly string address;
	} // class SendEmailVerification
} // namespace
