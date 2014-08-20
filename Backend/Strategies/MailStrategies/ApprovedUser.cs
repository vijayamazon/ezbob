namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Globalization;
	using System.Collections.Generic;
	using Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ApprovedUser : ABrokerMailToo {
		#region method GetFromDB

		public static void GetFromDB(AMailStrategyBase oStrategy, out int nNumOfApprovals, out int nValidHours) {
			var sr = oStrategy.DB.GetFirst(
				"GetApprovalData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", oStrategy.CustomerId)
			);

			if (oStrategy.CustomerId != (int)sr["CustomerID"])
				throw new StrategyWarning(oStrategy, "no approval data found for customer " + oStrategy.CustomerData);

			nNumOfApprovals = sr["NumOfApprovals"];

			DateTime applyForLoan = sr["ApplyForLoan"];
			DateTime validFor = sr["ValidFor"];

			nValidHours = (int)(validFor - applyForLoan).TotalHours;
		} // GetFromDB

		#endregion method GetFromDB

		#region constructor

		public ApprovedUser(int customerId, decimal nLoanAmount, AConnection oDB, ASafeLog oLog) : base(customerId, true, oDB, oLog) {
			m_nLoanAmount = nLoanAmount;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Approved User"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			int nNumOfApprovals;
			int nValidHours;

			GetFromDB(this, out nNumOfApprovals, out nValidHours);

			Variables = new Dictionary<string, string> {
				{ "FirstName", CustomerData.FirstName },
				{ "LoanAmount", m_nLoanAmount.ToString(CultureInfo.InvariantCulture) },
				{ "ValidFor", nValidHours.ToString(CultureInfo.InvariantCulture) }
			};

			TemplateName = nNumOfApprovals == 1
				? "Mandrill - Approval (1st time)"
				: "Mandrill - Approval (not 1st time)";
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		private readonly decimal m_nLoanAmount;
	} // class ApprovedUser
} // namespace
