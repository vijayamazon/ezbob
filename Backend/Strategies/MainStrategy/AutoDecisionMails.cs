namespace Ezbob.Backend.Strategies.MainStrategy {
	using System.Globalization;
	using Ezbob.Backend.Strategies.MailStrategies.API;
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
				{ "RegistrationDate", dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture) },
				{ "userID", customerId.ToString(CultureInfo.InvariantCulture) },
				{ "Name", dataGatherer.AppEmail },
				{ "FirstName", dataGatherer.AppFirstName },
				{ "Surname", dataGatherer.AppSurname },
				{ "MP_Counter", dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture) },
				{ "MedalType", medalClassification.ToString() },
				{ "SystemDecision", autoDecisionResponse.SystemDecision.ToString() },
				{ "ApprovalAmount", autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture) },
				{ "RepaymentPeriod", dataGatherer.LoanOfferRepaymentPeriod.ToString(CultureInfo.InvariantCulture) },
				{ "InterestRate", autoDecisionResponse.InterestRate.ToString(CultureInfo.InvariantCulture) },
				{
					"OfferValidUntil", autoDecisionResponse.AppValidFor.HasValue
						? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{ "FirstName", dataGatherer.AppFirstName },
				{ "LoanAmount", autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture) },
				{
					"ValidFor", autoDecisionResponse.AppValidFor.HasValue
						? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			};

			mailer.Send(
				"Mandrill - Approval (" + (isFirstLoan ? "" : "not ") + "1st time)",
				customerMailVariables,
				new Addressee(dataGatherer.AppEmail)
			);
		} // SendApprovalMails

		private void SendBankBasedApprovalMails() {
			mailer.Send("Mandrill - User is approved", new Dictionary<string, string> {
				{ "ApprovedReApproved", "Approved" },
				{ "RegistrationDate", dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture) },
				{ "userID", customerId.ToString(CultureInfo.InvariantCulture) },
				{ "Name", dataGatherer.AppEmail },
				{ "FirstName", dataGatherer.AppFirstName },
				{ "Surname", dataGatherer.AppSurname },
				{ "MP_Counter", dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture) },
				{ "MedalType", medalClassification.ToString() },
				{ "SystemDecision", autoDecisionResponse.SystemDecision.ToString() },
				{
					"ApprovalAmount",
					autoDecisionResponse.BankBasedAutoApproveAmount.ToString(CultureInfo.InvariantCulture)
				},
				{ "RepaymentPeriod", autoDecisionResponse.RepaymentPeriod.ToString(CultureInfo.InvariantCulture) },
				{ "InterestRate", dataGatherer.LoanOfferInterestRate.ToString(CultureInfo.InvariantCulture) },
				{
					"OfferValidUntil", autoDecisionResponse.AppValidFor.HasValue
						? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{ "FirstName", dataGatherer.AppFirstName },
				{
					"LoanAmount",
					autoDecisionResponse.BankBasedAutoApproveAmount.ToString(CultureInfo.InvariantCulture)
				},
				{ "ValidFor", autoDecisionResponse.AppValidFor.HasValue
					? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
					: string.Empty
				}
			};

			mailer.Send(
				"Mandrill - Approval (" + (isFirstLoan ? "" : "not ") + "1st time)",
				customerMailVariables,
				new Addressee(dataGatherer.AppEmail)
			);
		} // SendBankBasedApprovalMails

		private void SendReApprovalMails() {
			mailer.Send("Mandrill - User is re-approved", new Dictionary<string, string> {
				{ "ApprovedReApproved", "Re-Approved" },
				{ "RegistrationDate", dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture) },
				{ "userID", customerId.ToString(CultureInfo.InvariantCulture) },
				{ "Name", dataGatherer.AppEmail },
				{ "FirstName", dataGatherer.AppFirstName },
				{ "Surname", dataGatherer.AppSurname },
				{ "MP_Counter", dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture) },
				{ "MedalType", medalClassification.ToString() },
				{ "SystemDecision", autoDecisionResponse.SystemDecision.ToString() },
				{ "ApprovalAmount", offeredCreditLine.ToString(CultureInfo.InvariantCulture) },
				{ "RepaymentPeriod", autoDecisionResponse.RepaymentPeriod.ToString(CultureInfo.InvariantCulture) },
				{ "InterestRate", autoDecisionResponse.InterestRate.ToString(CultureInfo.InvariantCulture) },
				{
					"OfferValidUntil", autoDecisionResponse.AppValidFor.HasValue
						? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			});

			var customerMailVariables = new Dictionary<string, string> {
				{ "FirstName", dataGatherer.AppFirstName },
				{ "LoanAmount", autoDecisionResponse.AutoApproveAmount.ToString(CultureInfo.InvariantCulture) },
				{
					"ValidFor", autoDecisionResponse.AppValidFor.HasValue
						? autoDecisionResponse.AppValidFor.Value.ToString(CultureInfo.InvariantCulture)
						: string.Empty
				}
			};

			mailer.Send(
				dataGatherer.IsAlibaba ? "Mandrill - Alibaba - Approval" : "Mandrill - Approval (not 1st time)",
				customerMailVariables,
				new Addressee(dataGatherer.AppEmail)
			);
		} // SendReApprovalMails

		private void SendWaitingForDecisionMail() {
			mailer.Send("Mandrill - User is waiting for decision", new Dictionary<string, string> {
				{ "RegistrationDate", dataGatherer.AppRegistrationDate.ToString(CultureInfo.InvariantCulture) },
				{ "userID", customerId.ToString(CultureInfo.InvariantCulture) },
				{ "Name", dataGatherer.AppEmail },
				{ "FirstName", dataGatherer.AppFirstName },
				{ "Surname", dataGatherer.AppSurname },
				{ "MP_Counter", dataGatherer.AllMPsNum.ToString(CultureInfo.InvariantCulture) },
				{ "MedalType", medalClassification.ToString() },
				{ "SystemDecision", "WaitingForDecision" }
			});
		} // SendWaitingForDecisionMail
	}
}
