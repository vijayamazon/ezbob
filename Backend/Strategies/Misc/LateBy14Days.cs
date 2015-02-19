namespace Ezbob.Backend.Strategies.Misc {
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Backend.Strategies.MailStrategies.Collection;

	public class LateBy14Days : AStrategy {
		public override string Name { get { return "Late by 14 days"; } } // Name

		public override void Execute() {
			DB.ForEachRowSafe((sr, bRowsetStart) => {
				bool is14DaysLate = sr["Is14DaysLate"];

				if (is14DaysLate)
					return ActionResult.Continue;

				int loanId = sr["LoanId"];
				string signDate = sr["SignDate"];
				string firstName = sr["FirstName"];
				decimal loanAmount = sr["LoanAmount"];
				decimal principal = sr["Principal"];
				decimal interest = sr["Interest"];
				decimal fees = sr["Fees"];
				decimal total = sr["Total"];
				string mail = sr["Email"];
				int customerId = sr["CustomerId"];
				var variables = new Dictionary<string, string> {
					{"FirstName", firstName},
					{"SignDate", signDate},
					{"LoanAmount", loanAmount.ToString(CultureInfo.InvariantCulture)},
					{"Principal", principal.ToString(CultureInfo.InvariantCulture)},
					{"Interest", interest.ToString(CultureInfo.InvariantCulture)},
					{"Fees", fees.ToString(CultureInfo.InvariantCulture)},
					{"Total", total.ToString(CultureInfo.InvariantCulture)}
				};

				LateBy14DaysMail lateBy14DaysMail = new LateBy14DaysMail(customerId, "Mandrill - 14 days notification email", variables);
				lateBy14DaysMail.Execute();

				DB.ExecuteNonQuery("SetLateBy14Days", CommandSpecies.StoredProcedure, new QueryParameter("LoanId", loanId));

				return ActionResult.Continue;
			}, "GetLateBy14DaysAndUpdate", CommandSpecies.StoredProcedure);
		} // Execute
	} // class LateBy14Days
} // namespace
