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
			MedalResult medal,
			CustomerDetails customerDetails,
			AutoDecisionResponse autoDecisionResponse,
			bool sendToCustomer
		) {
			this.mailer = mailer;
			this.customerId = customerId;
			this.offeredCreditLine = offeredCreditLine;
			this.medal = medal;
			this.customerDetails = customerDetails;
			this.autoDecisionResponse = autoDecisionResponse;
			this.sendToCustomer = sendToCustomer;
		} // constructor

		public void SendEmails() {
			if (this.autoDecisionResponse.DecidedToReject)
				new RejectUser(this.customerId, this.sendToCustomer).Execute();
			else if (this.autoDecisionResponse.IsAutoApproval)
				SendApprovalMails();
			else if (this.autoDecisionResponse.IsAutoBankBasedApproval)
				SendBankBasedApprovalMails();
			else if (this.autoDecisionResponse.IsAutoReApproval)
				SendReApprovalMails();
			else if (!this.autoDecisionResponse.HasAutoDecided)
				SendWaitingForDecisionMail();
		} // SendEmails

		private void SendApprovalMails() {
			this.mailer.Send("Mandrill - User is approved", new Dictionary<string, string> {
				{ "ApprovedReApproved", "Approved" },
				{ "RegistrationDate", this.customerDetails.AppRegistrationDate.DateStr() },
				{ "userID", this.customerId.ToString(Library.Instance.Culture) },
				{ "Name", this.customerDetails.AppEmail },
				{ "FirstName", this.customerDetails.AppFirstName },
				{ "Surname", this.customerDetails.AppSurname },
				{ "MP_Counter", this.customerDetails.AllMPsNum.ToString(Library.Instance.Culture) },
				{ "MedalType", this.medal.MedalClassification.ToString() },
				{ "SystemDecision", this.autoDecisionResponse.SystemDecision.ToString() },
				{ "ApprovalAmount", this.autoDecisionResponse.AutoApproveAmount.ToString(Library.Instance.Culture) },
				{ "RepaymentPeriod", this.autoDecisionResponse.RepaymentPeriod.ToString(Library.Instance.Culture) },
				{ "InterestRate", this.autoDecisionResponse.InterestRate.ToString(Library.Instance.Culture) },
				{
					"OfferValidUntil", this.autoDecisionResponse.AppValidFor.HasValue
						? this.autoDecisionResponse.AppValidFor.Value.ToString(Library.Instance.Culture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{ "FirstName", this.customerDetails.AppFirstName },
				{ "LoanAmount", this.autoDecisionResponse.AutoApproveAmount.ToString(Library.Instance.Culture) },
				{
					"ValidFor", this.autoDecisionResponse.AppValidFor.HasValue
						? this.autoDecisionResponse.AppValidFor.Value.ToString(Library.Instance.Culture)
						: string.Empty
				}
			};

			var isFirstLoan = this.customerDetails.NumOfLoans == 0;

			AutomationDecsionMails automationDecsionMails = new AutomationDecsionMails(this.customerId,
				"Mandrill - Approval (" + (isFirstLoan ? "" : "not ") + "1st time)",
				customerMailVariables
			);
			automationDecsionMails.Execute();
		} // SendApprovalMails

		private void SendBankBasedApprovalMails() {
			this.mailer.Send("Mandrill - User is approved", new Dictionary<string, string> {
				{ "ApprovedReApproved", "Approved" },
				{ "RegistrationDate", this.customerDetails.AppRegistrationDate.ToString(Library.Instance.Culture) },
				{ "userID", this.customerId.ToString(Library.Instance.Culture) },
				{ "Name", this.customerDetails.AppEmail },
				{ "FirstName", this.customerDetails.AppFirstName },
				{ "Surname", this.customerDetails.AppSurname },
				{ "MP_Counter", this.customerDetails.AllMPsNum.ToString(Library.Instance.Culture) },
				{ "MedalType", this.medal.MedalClassification.ToString() },
				{ "SystemDecision", this.autoDecisionResponse.SystemDecision.ToString() },
				{
					"ApprovalAmount", this.autoDecisionResponse.BankBasedAutoApproveAmount.ToString(Library.Instance.Culture)
				},
				{ "RepaymentPeriod", this.autoDecisionResponse.RepaymentPeriod.ToString(Library.Instance.Culture) },
				{ "InterestRate", this.autoDecisionResponse.InterestRate.ToString(Library.Instance.Culture) },
				{
					"OfferValidUntil", this.autoDecisionResponse.AppValidFor.HasValue
						? this.autoDecisionResponse.AppValidFor.Value.ToString(Library.Instance.Culture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{ "FirstName", this.customerDetails.AppFirstName },
				{
					"LoanAmount", this.autoDecisionResponse.BankBasedAutoApproveAmount.ToString(Library.Instance.Culture)
				},
				{ "ValidFor", this.autoDecisionResponse.AppValidFor.HasValue
					? this.autoDecisionResponse.AppValidFor.Value.ToString(Library.Instance.Culture)
					: string.Empty
				}
			};

			var isFirstLoan = this.customerDetails.NumOfLoans == 0;

			AutomationDecsionMails automationDecsionMails = new AutomationDecsionMails(this.customerId,
				"Mandrill - Approval (" + (isFirstLoan ? "" : "not ") + "1st time)",
				customerMailVariables
			);
			automationDecsionMails.Execute();
		} // SendBankBasedApprovalMails

		private void SendReApprovalMails() {
			this.mailer.Send("Mandrill - User is re-approved", new Dictionary<string, string> {
				{ "ApprovedReApproved", "Re-Approved" },
				{ "RegistrationDate", this.customerDetails.AppRegistrationDate.ToString(Library.Instance.Culture) },
				{ "userID", this.customerId.ToString(Library.Instance.Culture) },
				{ "Name", this.customerDetails.AppEmail },
				{ "FirstName", this.customerDetails.AppFirstName },
				{ "Surname", this.customerDetails.AppSurname },
				{ "MP_Counter", this.customerDetails.AllMPsNum.ToString(Library.Instance.Culture) },
				{ "MedalType", this.medal.MedalClassification.ToString() },
				{ "SystemDecision", this.autoDecisionResponse.SystemDecision.ToString() },
				{ "ApprovalAmount", this.offeredCreditLine.ToString(Library.Instance.Culture) },
				{ "RepaymentPeriod", this.autoDecisionResponse.RepaymentPeriod.ToString(Library.Instance.Culture) },
				{ "InterestRate", this.autoDecisionResponse.InterestRate.ToString(Library.Instance.Culture) },
				{
					"OfferValidUntil", this.autoDecisionResponse.AppValidFor.HasValue
						? this.autoDecisionResponse.AppValidFor.Value.ToString(Library.Instance.Culture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{ "FirstName", this.customerDetails.AppFirstName },
				{ "LoanAmount", this.autoDecisionResponse.AutoApproveAmount.ToString(Library.Instance.Culture) },
				{
					"ValidFor", this.autoDecisionResponse.AppValidFor.HasValue
						? this.autoDecisionResponse.AppValidFor.Value.ToString(Library.Instance.Culture)
						: string.Empty
				}
			};

			AutomationDecsionMails automationDecsionMails = new AutomationDecsionMails(this.customerId,
				this.customerDetails.IsAlibaba ? "Mandrill - Alibaba - Approval" : "Mandrill - Approval (not 1st time)",
				customerMailVariables
			);
			automationDecsionMails.Execute();
		} // SendReApprovalMails

		private void SendWaitingForDecisionMail() {
			this.mailer.Send("Mandrill - User is waiting for decision", new Dictionary<string, string> {
				{ "RegistrationDate", this.customerDetails.AppRegistrationDate.ToString(Library.Instance.Culture) },
				{ "userID", this.customerId.ToString(Library.Instance.Culture) },
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
		private readonly MedalResult medal;
		private readonly CustomerDetails customerDetails;
		private readonly AutoDecisionResponse autoDecisionResponse;
		private readonly bool sendToCustomer;
		private readonly StrategiesMailer mailer;
	} // class MainStrategyMails
} // namespace
