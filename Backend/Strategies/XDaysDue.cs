using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Ezbob.Database;
using Ezbob.Logger;

namespace EzBob.Backend.Strategies {
	using Web.Code;

	public class XDaysDue : AStrategy {
		#region constructor

		public XDaysDue(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
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
			DataTable dt = DB.ExecuteReader("GetCustomersFiveDaysDue", CommandSpecies.StoredProcedure);

			foreach (DataRow row in dt.Rows) {
				int loanScheduleId = int.Parse(row["id"].ToString());
				decimal amountDue = decimal.Parse(row["AmountDue"].ToString());
				string firstName = row["FirstName"].ToString();
				string mail = row["Email"].ToString();
				DateTime sceduledDate = DateTime.Parse(row["SceduledDate"].ToString());
				string creditCard = row["CreditCardNo"].ToString();

				string subject = string.Format("Dear {0}, your ezbob monthly automatic loan re-payment is due in 5 days", firstName);

				var variables = new Dictionary<string, string> {
					{"FirstName", firstName},
					{"AmountDueScalar", amountDue.ToString(CultureInfo.InvariantCulture)},
					{"Date", FormattingUtils.FormatDateToString(sceduledDate)},
					{"DebitCard", creditCard}
				};

				mailer.SendToCustomerAndEzbob(variables, mail, "Mandrill - 5 days notice", subject);

				DB.ExecuteNonQuery("UpdateFiveDaysDueMailSent",
					CommandSpecies.StoredProcedure,
					new QueryParameter("Id", loanScheduleId),
					new QueryParameter("UpdateFiveDaysDueMailSent", true)
				);
			} // for each

			dt = DB.ExecuteReader("GetCustomersTwoDaysDue", CommandSpecies.StoredProcedure);

			foreach (DataRow row in dt.Rows) {
				int loanScheduleId = int.Parse(row["id"].ToString());
				decimal amountDue = decimal.Parse(row["AmountDue"].ToString());
				string firstName = row["FirstName"].ToString();
				string mail = row["Email"].ToString();
				DateTime sceduledDate = DateTime.Parse(row["SceduledDate"].ToString());
				string creditCard = row["CreditCardNo"].ToString();

				string subject = string.Format("Dear {0}, your ezbob monthly automatic loan re-payment is due in 48 hours", firstName);

				var variables = new Dictionary<string, string> {
					{"FirstName", firstName},
					{"AmountDueScalar", amountDue.ToString(CultureInfo.InvariantCulture)},
					{"Date", FormattingUtils.FormatDateToString(sceduledDate)},
					{"DebitCard", creditCard}
				};

				mailer.SendToCustomerAndEzbob(variables, mail, "Mandrill - 2 days notice", subject);

				DB.ExecuteNonQuery("UpdateTwoDaysDueMailSent",
					CommandSpecies.StoredProcedure,
					new QueryParameter("Id", loanScheduleId),
					new QueryParameter("UpdateTwoDaysDueMailSent", true)
				);
			} // for each
		} // Execute

		#endregion property Execute

		private readonly StrategiesMailer mailer;
	} // class XDaysDue
} // namespace
