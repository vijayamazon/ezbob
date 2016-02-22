namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System;
	using System.Collections.Generic;
	using DbConstants;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Extensions;
	using Ezbob.Backend.Strategies.Alibaba;
	using Ezbob.Backend.Strategies.MailStrategies;
	using Ezbob.Backend.Strategies.MailStrategies.API;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.SalesForce;
	using Ezbob.Logger;
	using Ezbob.Utils.Extensions;
	using SalesForceLib.Models;

	internal class ExternalNotifier {
		public ExternalNotifier(
			StrategiesMailer mailer,
			int customerId,
			MedalResult medal,
			CustomerDetails customerDetails,
			AutoDecisionResponse autoDecisionResponse,
			bool sendToCustomer,
			ASafeLog log
		) {
			this.mailer = mailer;
			this.customerId = customerId;
			this.medal = medal;
			this.customerDetails = customerDetails;
			this.autoDecisionResponse = autoDecisionResponse;
			this.sendToCustomer = sendToCustomer;
			this.log = log.Safe();
		} // constructor

		public void Execute() {
			UpdateSalesForceOpportunity();

			if (this.customerDetails.IsAlibaba)
				UpdatePartnerAlibaba();

			if (this.autoDecisionResponse.DecidedToReject)
				new RejectUser(this.customerId, this.sendToCustomer).Execute();
			else if (this.autoDecisionResponse.IsAutoApproval)
				SendApprovalMails();
			else if (this.autoDecisionResponse.IsAutoReApproval)
				SendReApprovalMails();
			else if (!this.autoDecisionResponse.HasAutoDecided)
				SendWaitingForDecisionMail();
		} // Execute

		private void UpdateSalesForceOpportunity() {
			string customerEmail = this.customerDetails.AppEmail;

			new AddUpdateLeadAccount(customerEmail, this.customerId, false, false).Execute();

			if (!this.autoDecisionResponse.Decision.HasValue)
				return;

			switch (this.autoDecisionResponse.Decision.Value) {
			case DecisionActions.Approve:
			case DecisionActions.ReApprove:
				new UpdateOpportunity(this.customerId, new OpportunityModel {
					Email = customerEmail,
					Origin = this.customerDetails.Origin,
					ApprovedAmount = this.autoDecisionResponse.ApprovedAmount,
					ExpectedEndDate = this.autoDecisionResponse.AppValidFor,
					Stage = OpportunityStage.s90.DescriptionAttr(),
				}).Execute();
				break;

			case DecisionActions.Reject:
			case DecisionActions.ReReject:
				new UpdateOpportunity(this.customerId, new OpportunityModel {
					Email = customerEmail,
					Origin = this.customerDetails.Origin,
					DealCloseType = OpportunityDealCloseReason.Lost.ToString(),
					DealLostReason = "Auto " + this.autoDecisionResponse.Decision.Value.ToString(),
					CloseDate = DateTime.UtcNow,
				}).Execute();
				break;
			} // switch
		} // UpdateSalesForceOpportunity

		/// <summary>
		/// In case of auto decision occurred (RR, R, RA, A), 002 sent immediately.
		/// Otherwise, i.e. in the case of Waiting/Manual, 002 will be transmitted
		/// when underwriter makes manual decision from
		/// CustomersController SetDecision method.
		/// </summary>
		private void UpdatePartnerAlibaba() {
			DecisionActions autoDecision = this.autoDecisionResponse.Decision ?? DecisionActions.Waiting;

			//	Reject, Re-Reject, Re-Approve, Approve: 0001 + 0002 (auto decision is a final also)
			// other: 0001 
			switch (autoDecision) {
			case DecisionActions.ReReject:
			case DecisionActions.Reject:
			case DecisionActions.ReApprove:
			case DecisionActions.Approve:
				new DataSharing(this.customerId, AlibabaBusinessType.APPLICATION).Execute();
				new DataSharing(this.customerId, AlibabaBusinessType.APPLICATION_REVIEW).Execute();
				break;

			// auto not final
			case DecisionActions.Waiting:
				new DataSharing(this.customerId, AlibabaBusinessType.APPLICATION).Execute();
				break;

			default: // unknown auto decision status
				this.log.Alert("Auto decision invalid value {0} for customer {1}", autoDecision, this.customerId);
				break;
			} // switch
		} // UpdatePartnerAlibaba

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
				{ "ApprovalAmount", this.autoDecisionResponse.ApprovedAmount.ToString(Library.Instance.Culture) },
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
				{ "LoanAmount", this.autoDecisionResponse.ApprovedAmount.ToString(Library.Instance.Culture) },
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
				{ "ApprovalAmount", this.autoDecisionResponse.ApprovedAmount.ToString(Library.Instance.Culture) },
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
				{ "LoanAmount", this.autoDecisionResponse.ApprovedAmount.ToString(Library.Instance.Culture) },
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
		private readonly MedalResult medal;
		private readonly CustomerDetails customerDetails;
		private readonly AutoDecisionResponse autoDecisionResponse;
		private readonly bool sendToCustomer;
		private readonly StrategiesMailer mailer;
		private readonly ASafeLog log;
	} // class ExternalNotifier
} // namespace
