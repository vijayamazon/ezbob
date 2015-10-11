namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.LoanCalculator;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
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

          /* 
		   * TODO
		   * 
		   * DB.ForEachRowSafe((sr, bRowsetStart) =>
            {
                NL_SendMailAndMarkDB("Mandrill - 5 days notice", sr, "NL_UpdateFiveDaysDueMailSent", 5);
                return ActionResult.Continue;
            }, "NL_GetCustomersFiveDaysDue", CommandSpecies.StoredProcedure);

            DB.ForEachRowSafe((sr, bRowsetStart) =>
            {
                NL_SendMailAndMarkDB("Mandrill - 2 days notice", sr, "NL_UpdateTwoDaysDueMailSent", 2);
                return ActionResult.Continue;
            }, "NL_GetCustomersTwoDaysDue", CommandSpecies.StoredProcedure);*/
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

        private void NL_SendMailAndMarkDB(string templateName, SafeReader sr, string spName, int dayNum) {

            int loanId = sr["LoanID"];
            //decimal amountDue = sr["AmountDue"];
            string firstName = sr["FirstName"];
            //string mail = sr["Email"];
            //DateTime sceduledDate = sr["SceduledDate"];
            string creditCard = sr["CreditCardNo"];
            int customerId = sr["customerId"];
            //int loanScheduleId = sr["id"];

			var loanState = new LoanState(new NL_Model(customerId), loanId, DateTime.UtcNow);
            loanState.Execute();

			// TODO: revive
            // LegacyLoanCalculator calc = new LegacyLoanCalculator(loanState.CalcModel);

			// TODO: this is wrong usage of CreateScheduleAndPlan method.
			// It should use GetAmountToChargeForAutoCharger with relevant date.

			/*
            List<ScheduledItemWithAmountDue> schedules = calc.CreateScheduleAndPlan();

            ScheduledItemWithAmountDue schedule = schedules.First(x => x.Date == DateTime.UtcNow.AddDays(dayNum));

            var variables = new Dictionary<string, string> {
					{"FirstName", firstName},
					{"AmountDueScalar", schedule.Amount.ToString(CultureInfo.InvariantCulture)},
					{"Date", FormattingUtils.FormatDateToString(schedule.Date)},
					{"DebitCard", creditCard}
				};

            XDaysDueMails xDaysDueMails = new XDaysDueMails(customerId, templateName, variables);
            xDaysDueMails.Execute();

            DB.ExecuteNonQuery(spName,
                CommandSpecies.StoredProcedure,
                new QueryParameter("@LoanID", loanId),
                new QueryParameter("@Date", DateTime.UtcNow.AddDays(dayNum))
                );
			*/

            //TODO update loan schedule x days due 
            Log.Info("update loan schedule x days due for customer {0}", customerId);
        }
	} // class XDaysDue
} // namespace
