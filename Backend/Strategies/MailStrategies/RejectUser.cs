namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class RejectUser : ABrokerMailToo {
		public RejectUser(int customerId, bool bSendToCustomer, AConnection oDB, ASafeLog oLog) : base(customerId, bSendToCustomer, oDB, oLog) {
		} // constructor

		public override string Name { get { return "Reject User"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			if (ConfigManager.CurrentValues.Instance.RejectionPartnersCities.Value.Contains("all") ||
			    ConfigManager.CurrentValues.Instance.RejectionPartnersCities.Value.Contains(CustomerData.City))
			{
				TemplateName = "Mandrill - Rejection partners email";
			}
			else
			{
				TemplateName = "Mandrill - Rejection email";
			}
			
			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName},
					{"EzbobAccount", "https://app.ezbob.com/Customer/Profile"}
				};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables
	} // class RejectUser
} // namespace
