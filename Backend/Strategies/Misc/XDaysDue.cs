namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB.NewLoan;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.NewLoan;
	using Ezbob.Database;

	public class XDaysDue : AStrategy {
		public override string Name {
			get { return "XDays Due"; }
		} // Name

		public DateTime nowTime { get; private set; }


		public override void Execute() {
			nowTime = DateTime.UtcNow;

			DB.ForEachRowSafe((sr, bRowsetStart) => {
				SendMailAndMarkDB("Mandrill - 5 days notice", sr, "UpdateFiveDaysDueMailSent", "UpdateFiveDaysDueMailSent");
				return ActionResult.Continue;
			}, // for each
				"GetCustomersFiveDaysDue",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Now", nowTime)
			);

			DB.ForEachRowSafe((sr, bRowsetStart) => {
				SendMailAndMarkDB("Mandrill - 2 days notice", sr, "UpdateTwoDaysDueMailSent", "UpdateTwoDaysDueMailSent");
				return ActionResult.Continue;
			}, "GetCustomersTwoDaysDue", CommandSpecies.StoredProcedure);

			DB.ForEachRowSafe((sr, bRowsetStart) => {
				NL_SendMailAndMarkDB(sr);
				return ActionResult.Continue;
			}, "NL_GetCustomersXDaysDue", CommandSpecies.StoredProcedure,
			new QueryParameter("Now", nowTime));

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

		private void NL_SendMailAndMarkDB(SafeReader sr) {

			int customerID = sr["CustomerId"];
			long loanID = sr["LoanID"];
			DateTime plannedDate = sr["PlannedDate"];
			int Xdays = sr["Xdays"]; // 2|5 indicate which type of notification to send
			long loanScheduleId = sr["LoanScheduleID"];

			NL_AddLog(LogType.Info, "NL_SendMailAndMarkDB", new object[] { customerID, loanID, Xdays, loanScheduleId, plannedDate, nowTime }, null, null, null);

			GetLoanState loanState = new GetLoanState(customerID, loanID, nowTime);
			loanState.Execute();
			var nlModel = loanState.Result;

			NL_LoanSchedules theSchedule = null;
			nlModel.Loan.Histories.ForEach(h => theSchedule = h.Schedule.FirstOrDefault(s => s.LoanScheduleID == loanScheduleId));

			if (theSchedule == null) {
				Log.Debug("Schedule for loan {0}, old {1} not found", loanID, sr["OldLoanID"]);
				NL_AddLog(LogType.Warn, "Schedule not found", new object[] { customerID, loanID, Xdays, loanScheduleId, plannedDate, nowTime }, null, null, null);
				return;
			}
			
			var variables = new Dictionary<string, string> {
                {"FirstName", sr["FirstName"]},
                {"AmountDueScalar", theSchedule.AmountDue.ToString(CultureInfo.InvariantCulture)},
                {"Date", FormattingUtils.FormatDateToString(plannedDate)},
                {"DebitCard", sr["CreditCardNo"]}
            };
			var templateName = (Xdays == 2) ? "Mandrill - 2 days notice" : "Mandrill - 5 days notice";
			XDaysDueMails xDaysDueMails = new XDaysDueMails(customerID, templateName, variables);
			//DON"T DELETE - in the futere email will be sent from here.!!! 
			//xDaysDueMails.Execute();

			var parameterName = (Xdays == 2) ? "TwoDaysDueMailSent" : "FiveDaysDueMailSent"; // new SP

			var queryParameteres = new QueryParameter[] {
                new QueryParameter("LoanScheduleID", loanScheduleId),
                new QueryParameter(parameterName, true)
            };

			NL_AddLog(LogType.Info, "Processing", new object[] { customerID, loanID, Xdays, loanScheduleId, plannedDate, nowTime }, new object[] { theSchedule, parameterName, templateName }, null, null);

			DB.ExecuteNonQuery("NL_LoanSchedulesUpdate", CommandSpecies.StoredProcedure, queryParameteres);

			Log.Info("update loanID {2} scheduleID {1} x days due for customer {0}", customerID, loanScheduleId, loanID);
		}
	} // class XDaysDue
} // namespace
