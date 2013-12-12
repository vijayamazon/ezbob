namespace EzBob.Backend.Strategies.MailStrategies
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.Collections.Generic;
	using DbConnection;

	public class ApprovedUser : MailStrategyBase
	{
		private readonly decimal loanAmount;

		public ApprovedUser(int customerId, decimal loanAmount)
			: base(customerId, true)
		{
			this.loanAmount = loanAmount;
		}

		public override void SetTemplateAndSubjectAndVariables()
		{
			Subject = string.Format("Congratulations {0}, £{1} is available to fund your business today", CustomerData.FirstName, loanAmount);

			DataTable dt = DbConnection.ExecuteSpReader("GetApprovalData", DbConnection.CreateParam("CustomerId", CustomerId));
			DataRow results = dt.Rows[0];
			int numOfApprovals = int.Parse(results["NumOfApprovals"].ToString());
			DateTime applyForLoan = DateTime.Parse(results["ApplyForLoan"].ToString());
			DateTime validFor = DateTime.Parse(results["ValidFor"].ToString());

			int validHours = (int)(validFor - applyForLoan).TotalHours;

			Variables = new Dictionary<string, string>
				{
					{"FirstName", CustomerData.FirstName},
					{"LoanAmount", loanAmount.ToString(CultureInfo.InvariantCulture)},
					{"ValidFor", validHours.ToString(CultureInfo.InvariantCulture)}
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
		}
	}
}
