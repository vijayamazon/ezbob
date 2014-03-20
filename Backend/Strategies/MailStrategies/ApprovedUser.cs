namespace EzBob.Backend.Strategies.MailStrategies {
	using System;
	using System.Data;
	using System.Globalization;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ApprovedUser : ABrokerMailToo {
		#region constructor

		public ApprovedUser(int customerId, decimal loanAmount, AConnection oDb, ASafeLog oLog) : base(customerId, true, oDb, oLog) {
			this.loanAmount = loanAmount;
		} // constructor

		#endregion constructor

		public override string Name { get { return "Approved User"; } } // Name

		#region method SetTemplateAndVariables

		protected override void SetTemplateAndVariables() {
			DataTable dt = DB.ExecuteReader(
				"GetApprovalData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", CustomerId)
			);

			if (dt.Rows.Count < 1)
				throw new StrategyException(this, "no approval data found for customer " + CustomerData);

			var sr = new SafeReader(dt.Rows[0]);
			int numOfApprovals = sr["NumOfApprovals"];
			DateTime applyForLoan = sr["ApplyForLoan"];
			DateTime validFor = sr["ValidFor"];

			double validHours = (validFor - applyForLoan).TotalHours;

			Variables = new Dictionary<string, string> {
				{"FirstName", CustomerData.FirstName},
				{"LoanAmount", loanAmount.ToString(CultureInfo.InvariantCulture)},
				{"ValidFor", ((int)validHours).ToString(CultureInfo.InvariantCulture)}
			};

			if (CustomerData.IsOffline)
			{
				TemplateName = numOfApprovals == 0
					? "Mandrill - Approval Offline (1st time)"
					: "Mandrill - Approval Offline (not 1st time)";
			}
			else
			{
				TemplateName = numOfApprovals == 0
					? "Mandrill - Approval (1st time)"
					: "Mandrill - Approval (not 1st time)";
			}
		} // SetTemplateAndVariables

		#endregion method SetTemplateAndVariables

		private readonly decimal loanAmount;
	} // class ApprovedUser
} // namespace
