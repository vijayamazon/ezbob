namespace Ezbob.Backend.Strategies.MainStrategy {
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Extensions;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Backend.Strategies.MedalCalculations;

	internal class MainStrategyMails {
		public MainStrategyMails(
			StrategiesMailer mailer,
			int customerId,
			int offeredCreditLine,
			LastOfferData lastOffer,
			MedalResult medal,
			CustomerDetails customerDetails,
			AutoDecisionResponse autoDecisionResponse,
			bool sendToCustomer
		) {
			this.mailer = mailer;
			this.customerId = customerId;
			this.offeredCreditLine = offeredCreditLine;
			this.lastOffer = lastOffer;
			this.medal = medal;
			this.customerDetails = customerDetails;
			this.autoDecisionResponse = autoDecisionResponse;
			this.sendToCustomer = sendToCustomer;
		} // constructor

		public void SendEmails() {
			if (autoDecisionResponse.DecidedToReject)
				new RejectUser(customerId, sendToCustomer).Execute();
			else if (autoDecisionResponse.IsAutoApproval)
				SendApprovalMails();
			else if (autoDecisionResponse.IsAutoBankBasedApproval)
				SendBankBasedApprovalMails();
			else if (autoDecisionResponse.IsAutoReApproval)
				SendReApprovalMails();
			else if (!autoDecisionResponse.HasAutomaticDecision)
				SendWaitingForDecisionMail();
		} // SendEmails

		private void SendApprovalMails() {
			mailer.Send("Mandrill - User is approved", new Dictionary<string, string> {
				{ "ApprovedReApproved", "Approved" },
				{ "RegistrationDate", this.customerDetails.AppRegistrationDate.DateStr() },
				{ "userID", this.customerId.ToString(Library.Instance.Culture) },
				{ "Name", this.customerDetails.AppEmail },
				{ "FirstName", this.customerDetails.AppFirstName },
				{ "Surname", this.customerDetails.AppSurname },
				{ "MP_Counter", this.customerDetails.AllMPsNum.ToString(Library.Instance.Culture) },
				{ "MedalType", this.medal.MedalClassification.ToString() },
				{ "SystemDecision", autoDecisionResponse.SystemDecision.ToString() },
				{ "ApprovalAmount", autoDecisionResponse.AutoApproveAmount.ToString(Library.Instance.Culture) },
				{ "RepaymentPeriod", this.lastOffer.LoanOfferRepaymentPeriod.ToString(Library.Instance.Culture) },
				{ "InterestRate", autoDecisionResponse.InterestRate.ToString(Library.Instance.Culture) },
				{
					"OfferValidUntil", autoDecisionResponse.AppValidFor.HasValue
						? autoDecisionResponse.AppValidFor.Value.ToString(Library.Instance.Culture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{ "FirstName", this.customerDetails.AppFirstName },
				{ "LoanAmount", autoDecisionResponse.AutoApproveAmount.ToString(Library.Instance.Culture) },
				{
					"ValidFor", autoDecisionResponse.AppValidFor.HasValue
						? autoDecisionResponse.AppValidFor.Value.ToString(Library.Instance.Culture)
						: string.Empty
				}
			};

			var isFirstLoan = this.customerDetails.NumOfLoans == 0;

			AutomationDecsionMails automationDecsionMails = new AutomationDecsionMails(
				customerId,
				"Mandrill - Approval (" + (isFirstLoan ? "" : "not ") + "1st time)",
				customerMailVariables
			);
			automationDecsionMails.Execute();
		} // SendApprovalMails

		private void SendBankBasedApprovalMails() {
			mailer.Send("Mandrill - User is approved", new Dictionary<string, string> {
				{ "ApprovedReApproved", "Approved" },
				{ "RegistrationDate", this.customerDetails.AppRegistrationDate.ToString(Library.Instance.Culture) },
				{ "userID", customerId.ToString(Library.Instance.Culture) },
				{ "Name", this.customerDetails.AppEmail },
				{ "FirstName", this.customerDetails.AppFirstName },
				{ "Surname", this.customerDetails.AppSurname },
				{ "MP_Counter", this.customerDetails.AllMPsNum.ToString(Library.Instance.Culture) },
				{ "MedalType", this.medal.MedalClassification.ToString() },
				{ "SystemDecision", autoDecisionResponse.SystemDecision.ToString() },
				{
					"ApprovalAmount",
					autoDecisionResponse.BankBasedAutoApproveAmount.ToString(Library.Instance.Culture)
				},
				{ "RepaymentPeriod", autoDecisionResponse.RepaymentPeriod.ToString(Library.Instance.Culture) },
				{ "InterestRate", this.lastOffer.LoanOfferInterestRate.ToString(Library.Instance.Culture) },
				{
					"OfferValidUntil", autoDecisionResponse.AppValidFor.HasValue
						? autoDecisionResponse.AppValidFor.Value.ToString(Library.Instance.Culture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{ "FirstName", this.customerDetails.AppFirstName },
				{
					"LoanAmount",
					autoDecisionResponse.BankBasedAutoApproveAmount.ToString(Library.Instance.Culture)
				},
				{ "ValidFor", autoDecisionResponse.AppValidFor.HasValue
					? autoDecisionResponse.AppValidFor.Value.ToString(Library.Instance.Culture)
					: string.Empty
				}
			};

			var isFirstLoan = this.customerDetails.NumOfLoans == 0;

			AutomationDecsionMails automationDecsionMails = new AutomationDecsionMails(
				customerId,
				"Mandrill - Approval (" + (isFirstLoan ? "" : "not ") + "1st time)",
				customerMailVariables
			);
			automationDecsionMails.Execute();
		} // SendBankBasedApprovalMails

		private void SendReApprovalMails() {
			mailer.Send("Mandrill - User is re-approved", new Dictionary<string, string> {
				{ "ApprovedReApproved", "Re-Approved" },
				{ "RegistrationDate", this.customerDetails.AppRegistrationDate.ToString(Library.Instance.Culture) },
				{ "userID", customerId.ToString(Library.Instance.Culture) },
				{ "Name", this.customerDetails.AppEmail },
				{ "FirstName", this.customerDetails.AppFirstName },
				{ "Surname", this.customerDetails.AppSurname },
				{ "MP_Counter", this.customerDetails.AllMPsNum.ToString(Library.Instance.Culture) },
				{ "MedalType", this.medal.MedalClassification.ToString() },
				{ "SystemDecision", autoDecisionResponse.SystemDecision.ToString() },
				{ "ApprovalAmount", offeredCreditLine.ToString(Library.Instance.Culture) },
				{ "RepaymentPeriod", autoDecisionResponse.RepaymentPeriod.ToString(Library.Instance.Culture) },
				{ "InterestRate", autoDecisionResponse.InterestRate.ToString(Library.Instance.Culture) },
				{
					"OfferValidUntil", autoDecisionResponse.AppValidFor.HasValue
						? autoDecisionResponse.AppValidFor.Value.ToString(Library.Instance.Culture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{ "FirstName", this.customerDetails.AppFirstName },
				{ "LoanAmount", autoDecisionResponse.AutoApproveAmount.ToString(Library.Instance.Culture) },
				{
					"ValidFor", autoDecisionResponse.AppValidFor.HasValue
						? autoDecisionResponse.AppValidFor.Value.ToString(Library.Instance.Culture)
						: string.Empty
				}
			};

			AutomationDecsionMails automationDecsionMails = new AutomationDecsionMails(customerId,
				this.customerDetails.IsAlibaba ? "Mandrill - Alibaba - Approval" : "Mandrill - Approval (not 1st time)",
				customerMailVariables
			);
			automationDecsionMails.Execute();
		} // SendReApprovalMails

		private void SendWaitingForDecisionMail() {
			mailer.Send("Mandrill - User is waiting for decision", new Dictionary<string, string> {
				{ "RegistrationDate", this.customerDetails.AppRegistrationDate.ToString(Library.Instance.Culture) },
				{ "userID", customerId.ToString(Library.Instance.Culture) },
				{ "Name", this.customerDetails.AppEmail },
				{ "FirstName", this.customerDetails.AppFirstName },
				{ "Surname", this.customerDetails.AppSurname },
				{ "MP_Counter", this.customerDetails.AllMPsNum.ToString(Library.Instance.Culture) },
				{ "MedalType", this.medal.MedalClassification.ToString() },
				{ "SystemDecision", "WaitingForDecision" }
			});
		} // SendWaitingForDecisionMail

		private readonly int customerId;
		private readonly int offeredCreditLine;
		private readonly LastOfferData lastOffer;
		private readonly MedalResult medal;
		private readonly CustomerDetails customerDetails;
		private readonly AutoDecisionResponse autoDecisionResponse;
		private readonly bool sendToCustomer;
		private readonly StrategiesMailer mailer;
	} // class MainStrategyMails
} // namespace
