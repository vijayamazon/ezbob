namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using API;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class RejectUser : ABrokerMailToo {
		public RejectUser(
			int customerId,
			bool bSendToCustomer,
			AConnection oDB,
			ASafeLog oLog
		) : base(customerId, bSendToCustomer, oDB, oLog) {
		} // constructor

		public override string Name { get { return "Reject User"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables()
		{
			if (CustomerData.IsAlibaba)
			{
				TemplateName = "Mandrill - Alibaba rejection email";
			}
			else if (CurrentValues.Instance.RejectionPartnersCities.Value.Contains("all") ||
				CurrentValues.Instance.RejectionPartnersCities.Value.Contains(CustomerData.City))
			{
				TemplateName = "Mandrill - Rejection partners email";
			}
			else
			{
				TemplateName = "Mandrill - Rejection email";
			} // if

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"EzbobAccount", CurrentValues.Instance.CustomerSite + "/Customer/Profile"}
			};
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		protected override void ActionAtEnd()
		{
			if (CustomerData.IsAlibaba)
			{
				var address = new Addressee(CurrentValues.Instance.AlibabaMailTo, CurrentValues.Instance.AlibabaMailCc);
				Log.Info("Sending Alibaba internal rejection mail");
				SendCostumeMail("Mandrill - Alibaba - Internal rejection email", Variables, new[] { address });
			}
		}
	} // class RejectUser
} // namespace
