namespace Ezbob.Backend.Strategies.MainStrategy {
	using System.Globalization;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.MailStrategies;

	partial class MainStrategy {
		private void SendEmails() {
			if (this.autoDecisionResponse.DecidedToReject)
				new RejectUser(this.customerId, true).Execute();
			else if (this.autoDecisionResponse.IsAutoApproval)
				SendApprovalMails();
			else if (this.autoDecisionResponse.IsAutoBankBasedApproval)
				SendBankBasedApprovalMails();
			else if (this.autoDecisionResponse.IsAutoReApproval)
				SendReApprovalMails();
			else if (!this.autoDecisionResponse.HasAutomaticDecision)
				SendWaitingForDecisionMail();
		} // SendEmails

		private void SendApprovalMails() {
			this.mailer.Send("Mandrill - User is approved", new Dictionary<string, string> {
				{ "ApprovedReApproved", "Approved" },
				{ "RegistrationDate", this.dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture) },
				{ "userID", this.customerId.ToString(CultureInfo.InvariantCulture) },
				{ "Name", this.dataGatherer.AppEmail },
				{ "FirstName", this.dataGatherer.AppFirstName },
				{ "Surname", this.dataGatherer.AppSurname },
				{ "MP_Counter", this.dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture) },
				{ "MedalType", this.medalClassification.ToString() },
				{ "SystemDecision", this.autoDecisionResponse.SystemDecision.ToString() },
				{ "ApprovalAmount", this.autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture) },
				{ "RepaymentPeriod", this.dataGatherer.LoanOfferRepaymentPeriod.ToString(CultureInfo.InvariantCulture) },
				{ "InterestRate", this.autoDecisionResponse.InterestRate.ToString(CultureInfo.InvariantCulture) },
				{
					"OfferValidUntil", this.autoDecisionResponse.AppValidFor.HasValue
						? this.autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{ "FirstName", this.dataGatherer.AppFirstName },
				{ "LoanAmount", this.autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture) },
				{
					"ValidFor", this.autoDecisionResponse.AppValidFor.HasValue
						? this.autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			};

			this.mailer.Send(
				"Mandrill - Approval (" + (this.isFirstLoan ? "" : "not ") + "1st time)",
				customerMailVariables,
				new Addressee(this.dataGatherer.AppEmail)
			);
		} // SendApprovalMails

		private void SendBankBasedApprovalMails() {
			this.mailer.Send("Mandrill - User is approved", new Dictionary<string, string> {
				{ "ApprovedReApproved", "Approved" },
				{ "RegistrationDate", this.dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture) },
				{ "userID", this.customerId.ToString(CultureInfo.InvariantCulture) },
				{ "Name", this.dataGatherer.AppEmail },
				{ "FirstName", this.dataGatherer.AppFirstName },
				{ "Surname", this.dataGatherer.AppSurname },
				{ "MP_Counter", this.dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture) },
				{ "MedalType", this.medalClassification.ToString() },
				{ "SystemDecision", this.autoDecisionResponse.SystemDecision.ToString() },
				{
					"ApprovalAmount",
					this.autoDecisionResponse.BankBasedAutoApproveAmount.ToString(CultureInfo.InvariantCulture)
				},
				{ "RepaymentPeriod", this.autoDecisionResponse.RepaymentPeriod.ToString(CultureInfo.InvariantCulture) },
				{ "InterestRate", this.dataGatherer.LoanOfferInterestRate.ToString(CultureInfo.InvariantCulture) },
				{
					"OfferValidUntil", this.autoDecisionResponse.AppValidFor.HasValue
						? this.autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{ "FirstName", this.dataGatherer.AppFirstName },
				{
					"LoanAmount",
					this.autoDecisionResponse.BankBasedAutoApproveAmount.ToString(CultureInfo.InvariantCulture)
				},
				{ "ValidFor", this.autoDecisionResponse.AppValidFor.HasValue
					? this.autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
					: string.Empty
				}
			};

			this.mailer.Send(
				"Mandrill - Approval (" + (this.isFirstLoan ? "" : "not ") + "1st time)",
				customerMailVariables,
				new Addressee(this.dataGatherer.AppEmail)
			);
		} // SendBankBasedApprovalMails

		private void SendReApprovalMails() {
			this.mailer.Send("Mandrill - User is re-approved", new Dictionary<string, string> {
				{ "ApprovedReApproved", "Re-Approved" },
				{ "RegistrationDate", this.dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture) },
				{ "userID", this.customerId.ToString(CultureInfo.InvariantCulture) },
				{ "Name", this.dataGatherer.AppEmail },
				{ "FirstName", this.dataGatherer.AppFirstName },
				{ "Surname", this.dataGatherer.AppSurname },
				{ "MP_Counter", this.dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture) },
				{ "MedalType", this.medalClassification.ToString() },
				{ "SystemDecision", this.autoDecisionResponse.SystemDecision.ToString() },
				{ "ApprovalAmount", this.offeredCreditLine.ToString(CultureInfo.InvariantCulture) },
				{ "RepaymentPeriod", this.autoDecisionResponse.RepaymentPeriod.ToString(CultureInfo.InvariantCulture) },
				{ "InterestRate", this.autoDecisionResponse.InterestRate.ToString(CultureInfo.InvariantCulture) },
				{
					"OfferValidUntil", this.autoDecisionResponse.AppValidFor.HasValue
						? this.autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{ "FirstName", this.dataGatherer.AppFirstName },
				{ "LoanAmount", this.autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture) },
				{
					"ValidFor", this.autoDecisionResponse.AppValidFor.HasValue
						? this.autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			};

			this.mailer.Send(
				this.dataGatherer.IsAlibaba ? "Mandrill - Alibaba - Approval" : "Mandrill - Approval (not 1st time)",
				customerMailVariables,
				new Addressee(this.dataGatherer.AppEmail)
			);
		} // SendReApprovalMails

		private void SendWaitingForDecisionMail() {
			this.mailer.Send("Mandrill - User is waiting for decision", new Dictionary<string, string> {
				{ "RegistrationDate", this.dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture) },
				{ "userID", this.customerId.ToString(CultureInfo.InvariantCulture) },
				{ "Name", this.dataGatherer.AppEmail },
				{ "FirstName", this.dataGatherer.AppFirstName },
				{ "Surname", this.dataGatherer.AppSurname },
				{ "MP_Counter", this.dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture) },
				{ "MedalType", this.medalClassification.ToString() },
				{ "SystemDecision", "WaitingForDecision" }
			});
		} // SendWaitingForDecisionMail
	}
}
