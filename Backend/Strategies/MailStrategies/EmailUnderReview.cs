namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class EmailUnderReview : ABrokerMailToo {
		#region constructor

		public EmailUnderReview(int customerId, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
		} // constructor

		#endregion constructor

		public override string Name { get { return "Email Under Review"; } }

		#region method LoadRecipientData

		protected override void LoadRecipientData() {
			base.LoadRecipientData();

			if (CustomerData.IsWhiteLabel) {
				SendToCustomer = false;
			}
		}

		#endregion method LoadRecipientData

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			TemplateName = "Mandrill - Application completed under review";

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName}
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables
	} // class EmailUnderReview
} // namespace
