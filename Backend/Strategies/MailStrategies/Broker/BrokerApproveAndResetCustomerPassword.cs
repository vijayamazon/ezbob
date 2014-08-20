namespace EzBob.Backend.Strategies.MailStrategies {
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BrokerApproveAndResetCustomerPassword : AMailStrategyBase {
		#region constructor

		public BrokerApproveAndResetCustomerPassword(
			int nCustomerID,
			decimal nLoanAmount,
			AConnection oDB,
			ASafeLog oLog
		) : base(nCustomerID, true, oDB, oLog) {
			m_nLoanAmount = nLoanAmount;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "BrokerApproveAndResetCustomerPassword"; }
		} // Name

		#endregion property Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			int nNumOfApprovals;
			int nValidHours;

			ApprovedUser.GetFromDB(this, out nNumOfApprovals, out nValidHours);

			Variables = new Dictionary<string, string> {
				{ "FirstName", CustomerData.FirstName },
				{ "Link", BrokerForceResetCustomerPassword.GetFromDB(this) },
				{ "LoanAmount", m_nLoanAmount.ToString(CultureInfo.InvariantCulture) },
				{ "ValidFor", nValidHours.ToString(CultureInfo.InvariantCulture) }
			};

			TemplateName = "Broker approve and reset customer password";
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		private readonly decimal m_nLoanAmount;
	} // class BrokerApproveAndResetCustomerPassword
} // namespace
