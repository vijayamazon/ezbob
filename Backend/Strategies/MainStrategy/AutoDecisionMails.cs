namespace Ezbob.Backend.Strategies.MainStrategy {
	using System.Collections.Generic;
	using System.Linq;
	using DbConstants;
	using Ezbob.Backend.Strategies.MailStrategies;

	partial class MainStrategy {
		private void SendEmails() {
			if (autoDecisionResponse.DecidedToReject) {
				bool sendToCustomer = true;
				var customer = _customers.ReallyTryGet(customerId);
				if (customer != null) {
					int numOfPreviousApprovals = customer.DecisionHistory.Count(x => x.Action == DecisionActions.Approve);
					if (customer.FilledByBroker && numOfPreviousApprovals == 0) {
						sendToCustomer = false;
					}
				}
				new RejectUser(customerId, sendToCustomer).Execute();
			} else if (autoDecisionResponse.IsAutoApproval)
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
				{ "MedalType", medalClassification.ToString() },
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
			AutomationDecsionMails automationDecsionMails = new AutomationDecsionMails(customerId, "Mandrill - Approval (" + (isFirstLoan ? "" : "not ") + "1st time)", customerMailVariables);
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
				{ "MedalType", medalClassification.ToString() },
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
			AutomationDecsionMails automationDecsionMails = new AutomationDecsionMails(customerId, "Mandrill - Approval (" + (isFirstLoan ? "" : "not ") + "1st time)", customerMailVariables);
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
				{ "MedalType", medalClassification.ToString() },
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
				this.customerDetails.IsAlibaba ? "Mandrill - Alibaba - Approval" : "Mandrill - Approval (not 1st time)", customerMailVariables);
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
				{ "MedalType", medalClassification.ToString() },
				{ "SystemDecision", "WaitingForDecision" }
			});
		} // SendWaitingForDecisionMail
	}
}
