namespace Ezbob.Backend.Strategies.Misc {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Ezbob.Backend.Models;
    using Ezbob.Backend.Strategies.MailStrategies;
    using Ezbob.Backend.Strategies.NewLoan;
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
              
            DB.ForEachRowSafe((sr, bRowsetStart) =>{
                NL_SendMailAndMarkDB(sr);
                return ActionResult.Continue;
            }, "NL_GetCustomersXDaysDue", CommandSpecies.StoredProcedure,
            new QueryParameter("Now", DateTime.UtcNow));

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

        private void NL_SendMailAndMarkDB( SafeReader sr) {

            int loanId = sr["LoanID"];
            string firstName = sr["FirstName"];
            DateTime plannedDate = sr["PlannedDate"];
            string creditCard = sr["CreditCardNo"];
            int customerId = sr["CustomerId"];
            int Xdays = sr["Xdays"]; // 2|5 indicate which type of notification to send
            int loanScheduleId = sr["LoanScheduleID"];

            //TODO : Replace by real value from Calculator (schedule items fully paid via calculator case "amountDue <= 0" skip).
            //DON"T DELETE - in the futere email will be sent from here.!!! 
            //var amountDue = 1000;
            //var variables = new Dictionary<string, string> {
            //    {"FirstName", firstName},
            //    {"AmountDueScalar", amountDue.ToString(CultureInfo.InvariantCulture)},
            //    {"Date", FormattingUtils.FormatDateToString(plannedDate)},
            //    {"DebitCard", creditCard}
            //};
            //var templateName = (Xdays == 2) ? "Mandrill - 2 days notice" : "Mandrill - 5 days notice";
            //XDaysDueMails xDaysDueMails = new XDaysDueMails(customerId, templateName, variables);
            //xDaysDueMails.Execute();

            var parameterName = (Xdays == 2) ? "TwoDaysDueMailSent" : "FiveDaysDueMailSent"; // new SP

            var queryParameteres = new QueryParameter[] {
                new QueryParameter("LoanScheduleID", loanScheduleId),
                new QueryParameter(parameterName, true)
            };

            var strategy = new UpdateLoanSchedules(loanScheduleId, queryParameteres, loanId);
            strategy.Execute();

            Log.Info("update loan schedule x days due for customer {0}", customerId);
        }
	} // class XDaysDue
} // namespace
