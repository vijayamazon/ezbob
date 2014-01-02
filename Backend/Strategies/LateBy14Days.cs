using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies {
	public class LateBy14Days : AStrategy {
		public LateBy14Days(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog) {
			mailer = new StrategiesMailer(DB, Log);
		} // constructor

		public override string Name { get { return "Late by 14 days"; } } // Name

		public override void Execute() {
			DataTable dt = DB.ExecuteReader("GetLateBy14DaysAndUpdate", CommandSpecies.StoredProcedure);

			foreach (DataRow row in dt.Rows) {
				var sr = new SafeReader(row);
				bool is14DaysLate = sr.Bool("Is14DaysLate");

				if (is14DaysLate)
					continue;

				int loanId = sr.Int("LoanId");
				string signDate = sr.String("SignDate");
				string firstName = sr.String("FirstName");
				decimal loanAmount = sr.Decimal("LoanAmount");
				decimal principal = sr.Decimal("Principal");
				decimal interest = sr.Decimal("Interest");
				decimal fees = sr.Decimal("Fees");
				decimal total = sr.Decimal("Total");
				string mail = sr.String("Email");

				var variables = new Dictionary<string, string> {
					{"FirstName", firstName},
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
