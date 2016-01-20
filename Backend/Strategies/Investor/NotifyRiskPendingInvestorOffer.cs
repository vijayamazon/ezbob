namespace Ezbob.Backend.Strategies.Investor {
	using System;
	using ConfigManager;
	using Ezbob.Backend.Strategies.MailStrategies;
	using MailApi;

	public class NotifyRiskPendingInvestorOffer : AStrategy {
		private readonly int customerID;
		private readonly decimal approvedAmount;
		private readonly DateTime offerValidUntil;

		public NotifyRiskPendingInvestorOffer(int customerID, decimal approvedAmount, DateTime offerValidUntil) {
			this.customerID = customerID;
			this.offerValidUntil = offerValidUntil;
			this.approvedAmount = approvedAmount;
		}//ctor

		public override string Name { get { return "NotifyRiskPendingInvestorOffer"; } }

		public override void Execute() {
			if (string.IsNullOrEmpty(CurrentValues.Instance.PendingInvestorNoficationReciever.Value)) {
				Log.Info("Not sending NotifyRiskPendingInvestorOffer the PendingInvestorNoficationReciever config is null or empty");
				return;
			}//if

			var customerData = new CustomerData(this, this.customerID, DB);
			customerData.Load();

			const string emailFormat =
			"<h2><a href={1}/{0}>{0}</a> {2} approved for £{3} pending Investor until {4}</h2>" +
			"<p>If this time limit will be reached - client's cash request will be automatically rejected.</p>" +
			"<p>Please find a solution to cover this loan before {4}</p>" +
			"<p>In order to find and match available funds for covering this loan request you can do the following:</p>" +
			"<ol>" +
				"<li>In case the loan request couldn't find available funds due to fully utilized allocations, as grade MAX, daily or monthly Max or other configured limitations on the currently active Investors - these limitations can be examined, in order to see if needs to be updated in order to allow automatic ability for covering a loan from this group.</li>" +
				"<li>In case all the relevant Investors have utilized their available Funds for funding - you can reach them in order to check whether they would like to transfer additional funds / to broaden their funding per period limitations in order to be able to continue to cover additional loans.</li>" +
				"<li>Taking this loan under Ezbob LTD Portfolio can always be considered as well.</li>" +
			"</ol>" +
			"<p>Please proceed  to UW - Manual matching between loan request to Investor page. <a href={5}>Click here</a> in order to choose a solution and match the pending request with funding.</p>";
			
			MailApi.Mail mailer = new Mail(CurrentValues.Instance.MandrillKey);

			mailer.Send(to: CurrentValues.Instance.PendingInvestorNoficationReciever,
				messageText: null,
				messageHtml: string.Format(emailFormat, 
					this.customerID, 
					"https://" + CurrentValues.Instance.UnderwriterSite + "/Underwriter/Customers#profile",
					customerData.FirstName,
					this.approvedAmount.ToString("#,#"),
					this.offerValidUntil.ToString("dd/MM/yy hh:mm"),
					"https://" + CurrentValues.Instance.UnderwriterSite + "/Underwriter/Customers#customers/pendingInvestor"
				),
				fromEmail: CurrentValues.Instance.MailSenderEmail, 
				fromName: CurrentValues.Instance.MailSenderName,
				subject: string.Format("{0} {1} approved for £{2} pending Investor until {3}", 
					this.customerID, 
					customerData.FirstName,
					this.approvedAmount.ToString("#,#"), 
					this.offerValidUntil.ToString("dd/MM/yy hh:mm")
				)
			);
		}//Execute
	}//NotifyRiskPendingInvestorOffer
}//ns
