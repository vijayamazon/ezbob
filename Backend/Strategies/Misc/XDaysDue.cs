namespace Ezbob.Backend.Strategies.Misc {
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Database;

	public class XDaysDue : AStrategy {
		public override string Name {
			get { return "XDays Due"; }
		} // Name

		public override void Execute() {
			DB.ForEachRowSafe((sr, bRowsetStart) => {
				int loanScheduleId = sr["id"];
				decimal amountDue = sr["AmountDue"];
				string firstName = sr["FirstName"];
				string mail = sr["Email"];
				DateTime sceduledDate = sr["SceduledDate"];
				string creditCard = sr["CreditCardNo"];
				int customerId = sr["customerId"];

				var variables = new Dictionary<string, string> {
					{"FirstName", firstName},
					{"AmountDueScalar", amountDue.ToString(CultureInfo.InvariantCulture)},
					{"Date", FormattingUtils.FormatDateToString(sceduledDate)},
					{"DebitCard", creditCard}
				};

				XDaysDueMails xDaysDueMails = new XDaysDueMails(customerId, "Mandrill - 5 days notice", variables);
				xDaysDueMails.Execute();

				DB.ExecuteNonQuery("UpdateFiveDaysDueMailSent",
					CommandSpecies.StoredProcedure,
					new QueryParameter("Id", loanScheduleId),
					new QueryParameter("UpdateFiveDaysDueMailSent", true)
				);

				return ActionResult.Continue;
			}, // for each
				"GetCustomersFiveDaysDue", 
				CommandSpecies.StoredProcedure,
				new QueryParameter("Now", DateTime.UtcNow)
			);

			DB.ForEachRowSafe((sr, bRowsetStart) => {
				int loanScheduleId = sr["id"];
				decimal amountDue = sr["AmountDue"];
				string firstName = sr["FirstName"];
				string mail = sr["Email"];
				DateTime sceduledDate = sr["SceduledDate"];
				string creditCard = sr["CreditCardNo"];
				int customerId = sr["customerId"];

				var variables = new Dictionary<string, string> {
					{"FirstName", firstName},
					{"AmountDueScalar", amountDue.ToString(CultureInfo.InvariantCulture)},
					{"Date", FormattingUtils.FormatDateToString(sceduledDate)},
					{"DebitCard", creditCard}
				};

				XDaysDueMails xDaysDueMails = new XDaysDueMails(customerId, "Mandrill - 2 days notice", variables);
				xDaysDueMails.Execute();

				DB.ExecuteNonQuery("UpdateTwoDaysDueMailSent",
					CommandSpecies.StoredProcedure,
					new QueryParameter("Id", loanScheduleId),
					new QueryParameter("UpdateTwoDaysDueMailSent", true)
				);

				return ActionResult.Continue;
			}, "GetCustomersTwoDaysDue", CommandSpecies.StoredProcedure);
		} // Execute
	} // class XDaysDue
} // namespace
