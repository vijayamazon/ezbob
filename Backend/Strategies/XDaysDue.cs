namespace EzBob.Backend.Strategies
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using Web.Code;
	using log4net;
	using DbConnection;

	public class XDaysDue
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(XDaysDue));
		private readonly StrategiesMailer mailer = new StrategiesMailer();

		public void Execute()
		{
			DataTable dt = DbConnection.ExecuteSpReader("GetCustomersFiveDaysDue");
			foreach (DataRow row in dt.Rows)
			{
				int loanScheduleId = int.Parse(row["id"].ToString());
				decimal amountDue = decimal.Parse(row["AmountDue"].ToString());
				string firstName = row["FirstName"].ToString();
				string mail = row["Email"].ToString();
				DateTime sceduledDate = DateTime.Parse(row["SceduledDate"].ToString());
				string creditCard = row["CreditCardNo"].ToString();

				string subject = string.Format("Dear {0}, your ezbob monthly automatic loan re-payment is due in 5 days", firstName);

				var variables = new Dictionary<string, string>
					{
						{"FirstName", firstName},
						{"AmountDueScalar", amountDue.ToString(CultureInfo.InvariantCulture)},
						{"Date", FormattingUtils.FormatDateToString(sceduledDate)},
						{"DebitCard", creditCard}
					};
				mailer.SendToCustomerAndEzbob(variables, mail, "Mandrill - 5 days notice", subject);

				DbConnection.ExecuteSpNonQuery("UpdateFiveDaysDueMailSent",
				                               DbConnection.CreateParam("Id", loanScheduleId),
											   DbConnection.CreateParam("UpdateFiveDaysDueMailSent", true));

			}

			dt = DbConnection.ExecuteSpReader("GetCustomersTwoDaysDue");
			foreach (DataRow row in dt.Rows)
			{
				int loanScheduleId = int.Parse(row["id"].ToString());
				decimal amountDue = decimal.Parse(row["AmountDue"].ToString());
				string firstName = row["FirstName"].ToString();
				string mail = row["Email"].ToString();
				DateTime sceduledDate = DateTime.Parse(row["SceduledDate"].ToString());
				string creditCard = row["CreditCardNo"].ToString();

				string subject = string.Format("Dear {0}, your ezbob monthly automatic loan re-payment is due in 48 hours", firstName);

				var variables = new Dictionary<string, string>
					{
						{"FirstName", firstName},
						{"AmountDueScalar", amountDue.ToString(CultureInfo.InvariantCulture)},
						{"Date", FormattingUtils.FormatDateToString(sceduledDate)},
						{"DebitCard", creditCard}
					};
				mailer.SendToCustomerAndEzbob(variables, mail, "Mandrill - 2 days notice", subject);

				DbConnection.ExecuteSpNonQuery("UpdateTwoDaysDueMailSent",
											   DbConnection.CreateParam("Id", loanScheduleId),
											   DbConnection.CreateParam("UpdateTwoDaysDueMailSent", true));
			}
		}
	}
}
