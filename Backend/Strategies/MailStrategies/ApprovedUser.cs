namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Globalization;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ApprovedUser : ABrokerMailToo {
		#region constructor

		public ApprovedUser(int customerId, decimal nLoanAmount, int nValidHours, bool isFirst, AConnection oDB, ASafeLog oLog) : base(customerId, false, oDB, oLog) {
			m_nLoanAmount = nLoanAmount;
			m_nValidHours = nValidHours;
			m_bIsFirst = isFirst;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Approved User"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {

			Variables = new Dictionary<string, string> {
				{ "FirstName", CustomerData.FirstName },
				{ "LoanAmount", m_nLoanAmount.ToString(CultureInfo.InvariantCulture) },
				{ "ValidFor", m_nValidHours.ToString(CultureInfo.InvariantCulture) }
			};

			if (m_bIsFirst)
			{
				if (CustomerData.IsCampaign)
				{
					TemplateName = "Mandrill - Approval Campaign (1st time)";
				}
				else
				{
					TemplateName = CustomerData.IsAlibaba ? "Mandrill - Alibaba - Approval (1st time)" : "Mandrill - Approval (1st time)";
				}
			}
			else
			{
				TemplateName = CustomerData.IsAlibaba ? "Mandrill - Alibaba - Approval (not 1st time)" : "Mandrill - Approval (not 1st time)";
			}
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		private readonly decimal m_nLoanAmount;
		private readonly int m_nValidHours;
		private readonly bool m_bIsFirst;
	} // class ApprovedUser
} // namespace
