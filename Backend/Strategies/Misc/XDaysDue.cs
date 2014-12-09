namespace Ezbob.Backend.Strategies.Misc {
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Backend.Models;
	using Ezbob.Database;

	public class XDaysDue : AStrategy {

		public XDaysDue() {
			mailer = new StrategiesMailer();
		} // constructor

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

				var variables = new Dictionary<string, string> {
					{"FirstName", firstName},
					{"AmountDueScalar", amountDue.ToString(CultureInfo.InvariantCulture)},
					{"Date", FormattingUtils.FormatDateToString(sceduledDate)},
					{"DebitCard", creditCard}
				};

				mailer.Send("Mandrill - 5 days notice", variables, new Addressee(mail));

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

				var variables = new Dictionary<string, string> {
					{"FirstName", firstName},
					{"AmountDueScalar", amountDue.ToString(CultureInfo.InvariantCulture)},
					{"Date", FormattingUtils.FormatDateToString(sceduledDate)},
					{"DebitCard", creditCard}
				};

				mailer.Send("Mandrill - 2 days notice", variables, new Addressee(mail));

				DB.ExecuteNonQuery("UpdateTwoDaysDueMailSent",
					CommandSpecies.StoredProcedure,
					new QueryParameter("Id", loanScheduleId),
					new QueryParameter("UpdateTwoDaysDueMailSent", true)
				);

				return ActionResult.Continue;
			}, "GetCustomersTwoDaysDue", CommandSpecies.StoredProcedure);
		} // Execute

		private readonly StrategiesMailer mailer;
	} // class XDaysDue
} // namespace
