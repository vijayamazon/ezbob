namespace EzBob.Backend.Strategies.Misc {
	using EzBob.Backend.Strategies.MailStrategies.API;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class XDaysDue : AStrategy {
		#region constructor

		public XDaysDue(AConnection oDb, ASafeLog oLog) : base(oDb, oLog) {
			mailer = new StrategiesMailer(DB, Log);
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "XDays Due"; }
		} // Name

		#endregion property Name

		#region property Execute

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

		#endregion property Execute

		private readonly StrategiesMailer mailer;
	} // class XDaysDue
} // namespace
