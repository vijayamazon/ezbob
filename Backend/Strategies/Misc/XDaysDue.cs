namespace Ezbob.Backend.Strategies.Misc {
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
                SendMailAndMarkDB("Mandrill - 5 days notice", sr, "UpdateFiveDaysDueMailSent", "UpdateFiveDaysDueMailSent");
				return ActionResult.Continue;
			}, // for each
				"GetCustomersFiveDaysDue", 
				CommandSpecies.StoredProcedure,
				new QueryParameter("Now", DateTime.UtcNow)
			);

			DB.ForEachRowSafe((sr, bRowsetStart) => {
                SendMailAndMarkDB("Mandrill - 2 days notice", sr, "UpdateTwoDaysDueMailSent", "UpdateTwoDaysDueMailSent");
				return ActionResult.Continue;
			}, "GetCustomersTwoDaysDue", CommandSpecies.StoredProcedure);
		} // Execute

        private void SendMailAndMarkDB(string templateName, SafeReader sr, string spName, string fieldName) {
            decimal amountDue = sr["AmountDue"];
            string firstName = sr["FirstName"];
            //string mail = sr["Email"];
            DateTime sceduledDate = sr["SceduledDate"];
            string creditCard = sr["CreditCardNo"];
            int customerId = sr["customerId"];
            int loanScheduleId = sr["id"];

            var variables = new Dictionary<string, string> {
					{"FirstName", firstName},
					{"AmountDueScalar", amountDue.ToString(CultureInfo.InvariantCulture)},
					{"Date", FormattingUtils.FormatDateToString(sceduledDate)},
					{"DebitCard", creditCard}
				};

            XDaysDueMails xDaysDueMails = new XDaysDueMails(customerId, templateName, variables);
            xDaysDueMails.Execute();

            DB.ExecuteNonQuery(spName,
                    CommandSpecies.StoredProcedure,
                    new QueryParameter("Id", loanScheduleId),
                    new QueryParameter(fieldName, true)
                );

            //TODO update loan schedule x days due 
            Log.Info("update loan schedule x days due for customer {0}", customerId);
        }
	} // class XDaysDue
} // namespace
