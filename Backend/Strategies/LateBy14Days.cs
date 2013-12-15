namespace EzBob.Backend.Strategies
{
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using DbConnection;
	using log4net;

	public class LateBy14Days
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(LateBy14Days));
		private readonly StrategiesMailer mailer = new StrategiesMailer();

		public void Execute()
		{
			DataTable dt = DbConnection.ExecuteSpReader("GetLateBy14DaysAndUpdate");
			foreach (DataRow row in dt.Rows)
			{
				int loanId = int.Parse(row["LoanId"].ToString());
				bool is14DaysLate = bool.Parse(row["Is14DaysLate"].ToString());
				string signDate = row["SignDate"].ToString();
				decimal loanAmount = decimal.Parse(row["LoanAmount"].ToString());
				decimal principal = decimal.Parse(row["Principal"].ToString());
				decimal interest = decimal.Parse(row["Interest"].ToString());
				decimal fees = decimal.Parse(row["Fees"].ToString());
				decimal total = decimal.Parse(row["Total"].ToString());
				string mail = row["Email"].ToString();

				if (is14DaysLate)
				{
					var variables = new Dictionary<string, string>
						{
							{"SignDate", signDate},
							{"LoanAmount", loanAmount.ToString(CultureInfo.InvariantCulture)},
							{"Principal", principal.ToString(CultureInfo.InvariantCulture)},
							{"Interest", interest.ToString(CultureInfo.InvariantCulture)},
							{"Fees", fees.ToString(CultureInfo.InvariantCulture)},
							{"Total", total.ToString(CultureInfo.InvariantCulture)}
						};
					mailer.SendToCustomerAndEzbob(variables, mail, "Mandrill - 14 days notification email", "Please contact us immediately in order to make full payment on all outstanding debts");

					DbConnection.ExecuteSpNonQuery("SetLateBy14Days", DbConnection.CreateParam("LoanId", loanId));
				}
			}
		}
	}
}
