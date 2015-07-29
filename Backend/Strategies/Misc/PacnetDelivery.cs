namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Threading.Tasks;
	using MailApi;
	using Ezbob.Backend.Models;

	public class PacnetDelivery  : AStrategy {
		/// <summary>
		/// Sending emails to pacnet office either in Dublin or in Vancouver with money transfer confirmation request 
		/// and starting to listen to answer for an hour 
		/// </summary>
		/// <param name="underwriterId">underwriter's system identifier</param>
		/// <param name="amount">amount of transferred money</param>
		public PacnetDelivery(int underwriterId, decimal amount) {
			this.underwriterId = underwriterId;
			this.amount = amount;
			this.irishEmails = ConfigManager.CurrentValues.Instance.PacnetDublinEmails;
			this.candianEmails = ConfigManager.CurrentValues.Instance.PacnetVancouverEmails;
			this.topUpSendingEmail = ConfigManager.CurrentValues.Instance.TopUpSendingEmail;
		} // constructor
		

		public override string Name {
			get {return "Send email to pacnet"; }
		} // Name
		

		public override void Execute(){

			Mail mail = new Mail();

			TimeZoneInfo dublinTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
			DateTime now = DateTime.UtcNow;

			DateTime irishTimeNow = TimeZoneInfo.ConvertTimeFromUtc(now, dublinTimeZone);

			//Irish pacnet office working hours 
			int openingHour = 8;
			int openingMinutes = 0;
			int closingHour = 15;
			int closingMinutes = 30;

			DateTime irishOfficeOpenMoment = now.Date.AddHours(openingHour).AddMinutes(openingMinutes);
			DateTime irishOfficeClosedMoment = now.Date.AddHours(closingHour).AddMinutes(closingMinutes);

			bool isIrishOfficeOpen = irishTimeNow > irishOfficeOpenMoment && irishTimeNow < irishOfficeClosedMoment;

			string emails = isIrishOfficeOpen ? irishEmails : candianEmails;

			string content = String.Format("Dear Madame/Sir, <br /><br />" +
				"We have transferred {0} to your account. Please confirm receiving and crediting the funds.<br /><br />" +
				"Best regards, <br />" +
				"Orange Money underwriter <br />" +
				"risk@ezbob.com" +
				"<br /> 0800-011-4787", FormattingUtils.FormatPoundsNoDecimals(amount));
			Log.Info("UW sending money transfer request email to top-up staff: " + emails);
			string subject = String.Format("Money Transfer Confirmation Request {0} UTC", now);
			mail.Send(emails, null, content, this.topUpSendingEmail, "Orange Money underwriter ", subject);

			var pacnet = new PacnetTopUpMailProcessor(underwriterId, amount, subject);

			pacnet.Execute();
		}

		private readonly int underwriterId;
		private readonly decimal amount;
		private readonly string irishEmails;
		private readonly string candianEmails;
		private readonly string topUpSendingEmail;
	}

}
