namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using Ezbob.Database;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using ConfigManager;
	using MailApi;
	using Ezbob.Backend.Models;


	public class TopUpDelivery : AStrategy {
		/// <summary>
		/// Sending sms/emails to top-up staff
		/// </summary>
		/// <param name="underwriterId">underwriter's system identifier</param>
		/// <param name="amount">amount of money transferred / to transfer</param>
		/// <param name="contentCase">message content identifier</param>
		public TopUpDelivery(int underwriterId, decimal amount, int contentCase) {
			this.underwriterId = underwriterId;
			this.amount = amount;
			this.contentCase = contentCase;
			this.topUpSendingEmail = ConfigManager.CurrentValues.Instance.TopUpSendingEmail;
		} // constructor
		
		public override string Name {
			get {return "Send email/SMS to top-up"; }
		} // Name

		public override void Execute() {

			var availableFunds = new GetAvailableFunds();
			availableFunds.Execute();
			var availFunds = availableFunds.AvailableFunds;
			var openOffers = availableFunds.ReservedAmount;
			var remainingFunds = availableFunds.AvailableFunds - availableFunds.ReservedAmount;

			switch (this.contentCase) {
			case 1:
				this.content = String.Format("Please make a transfer for {0} to Pacnet. \nAvailable funds: {1}; \nOpenoffers: {2}; \nRemaining funds: {3}",
				FormattingUtils.FormatPoundsNoDecimals(this.amount), FormattingUtils.FormatPoundsNoDecimals(availFunds),
				FormattingUtils.FormatPoundsNoDecimals(openOffers), FormattingUtils.FormatPoundsNoDecimals(remainingFunds));
				break;
			case 2:
				this.content = String.Format("There is no confirmation from Pacnet regarding money transfer of {0} for more than an hour.",
				FormattingUtils.FormatPoundsNoDecimals(this.amount));
				break;
			}

			if (string.IsNullOrEmpty(this.content)) {
				Log.Info("Empty content message is not sent");
				return;
			}

			List<string> emails = new List<string>();
			DB.ForEachRowSafe((sr, bRowsetStart) => {
				if (!string.IsNullOrEmpty(sr["SendMobilePhone"]) && !CurrentValues.Instance.SmsTestModeEnabled) {
					Log.Info("UW sending sms to top-up phone number {0}\n content {1}", sr["SendMobilePhone"], this.content);
					new SendSms(null, this.underwriterId, sr["SendMobilePhone"], this.content, sr["PhoneOriginIsrael"]).Execute();
				};
				if(!string.IsNullOrEmpty(sr["email"]))
					emails.Add(sr["email"]);
				return Database.ActionResult.Continue;
			}, "GetTopUpReceivers", CommandSpecies.StoredProcedure);

			SendEmail(emails);
		}

		private void SendEmail(List<string>  emails) {
			Mail mail = new Mail();
			string emailList = emails.Aggregate((a, b) => a + ";" + b);

			var mailContent = Regex.Replace(this.content, @"\n", "<br />");

			Log.Info("UW sending money transfer request email to top-up staff: " + emailList);
			mail.Send(emailList, null, mailContent, this.topUpSendingEmail, "ezbob UW", "Money Transfer"); 
		}
		
		private readonly int underwriterId;
		private string content;
		private readonly int contentCase;
		private readonly decimal amount;
		private readonly string topUpSendingEmail;
	}
}
