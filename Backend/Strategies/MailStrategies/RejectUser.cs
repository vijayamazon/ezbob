namespace Ezbob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using System.Globalization;
	using API;
	using ConfigManager;

	public class RejectUser : ABrokerMailToo {
		public RejectUser(int customerId, bool bSendToCustomer) : base(customerId, bSendToCustomer) {
		} // constructor

		public override string Name { get { return "Reject User"; } } // Name

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
				{ "FirstName", CustomerData.FirstName},
				{ "EzbobAccount", CustomerData.OriginSite + "/Customer/Profile"},
				{ "AlibabaId", CustomerData.AlibabaId.ToString(CultureInfo.InvariantCulture) },
				{ "RefNum", CustomerData.RefNum.ToString(CultureInfo.InvariantCulture) },
				{ "Surname", CustomerData.Surname.ToString(CultureInfo.InvariantCulture) },
				{ "RequestedLoanAmount", CustomerData.RequestedLoanAmount.ToString("#,#") },
				{ "ReportedAnnualTurnover", CustomerData.ReportedAnnualTurnover.ToString("#,#") }
			};
		} // SetTemplateAndVariables

		protected override void ActionAtEnd()
		{
			if (CustomerData.IsAlibaba)
			{
				var address = new Addressee(CurrentValues.Instance.AlibabaMailTo, CurrentValues.Instance.AlibabaMailCc, addSalesforceActivity:false);
				Log.Info("Sending Alibaba internal rejection mail");
				SendCostumeMail("Mandrill - Alibaba - Internal rejection email", Variables, new[] { address });
			}
		}
	} // class RejectUser
} // namespace
