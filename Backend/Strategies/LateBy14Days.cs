using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies {
	public class LateBy14Days : AStrategy {
		public LateBy14Days(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			mailer = new StrategiesMailer(DB, Log);
		} // constructor

		public override string Name { get { return "Late by 14 days"; } } // Name

		public void Execute() {
			DataTable dt = DB.ExecuteReader("GetLateBy14DaysAndUpdate", CommandSpecies.StoredProcedure);

			foreach (DataRow row in dt.Rows) {
				bool is14DaysLate = bool.Parse(row["Is14DaysLate"].ToString());

				if (!is14DaysLate)
					continue;

				int loanId = int.Parse(row["LoanId"].ToString());
				string signDate = row["SignDate"].ToString();
				decimal loanAmount = decimal.Parse(row["LoanAmount"].ToString());
				decimal principal = decimal.Parse(row["Principal"].ToString());
				decimal interest = decimal.Parse(row["Interest"].ToString());
				decimal fees = decimal.Parse(row["Fees"].ToString());
				decimal total = decimal.Parse(row["Total"].ToString());
				string mail = row["Email"].ToString();

				var variables = new Dictionary<string, string> {
					{"SignDate", signDate},
					{"LoanAmount", loanAmount.ToString(CultureInfo.InvariantCulture)},
					{"Principal", principal.ToString(CultureInfo.InvariantCulture)},
					{"Interest", interest.ToString(CultureInfo.InvariantCulture)},
					{"Fees", fees.ToString(CultureInfo.InvariantCulture)},
					{"Total", total.ToString(CultureInfo.InvariantCulture)}
				};

				mailer.SendToCustomerAndEzbob(variables, mail, "Mandrill - 14 days notification email", "Please contact us immediately in order to make full payment on all outstanding debts");

				DB.ExecuteNonQuery("SetLateBy14Days", CommandSpecies.StoredProcedure, new QueryParameter("LoanId", loanId));
			} // for
		} // Execute

		private readonly StrategiesMailer mailer;
	} // class LateBy14Days
} // namespace
