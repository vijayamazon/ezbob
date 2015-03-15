namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions {
	using System;
	using DbConstants;
	using EZBob.DatabaseLib.Model.Database;

	public class AutoDecisionResponse {
		public bool IsAutoBankBasedApproval { get; set; } //flag that means automation decided to give bank based approval

		public bool HasAutomaticDecision { get { return Decision != null; } } //auto reject re-reject approve re-approve (not silent mode)
		public DecisionActions? Decision { get; set; }

		public int? DecisionCode { get { return Decision == null ? (int?)null : (int)Decision; } }

		public bool IsAutoApproval { get { return Decision == DecisionActions.Approve; } }
		public bool IsAutoReApproval { get { return Decision == DecisionActions.ReApprove; } }
		public bool IsReRejected { get { return Decision == DecisionActions.ReReject; } } // flag that means customer automation decided to re-reject the client
		public bool IsRejected { get { return Decision == DecisionActions.Reject; } } // flag that means customer automation decided to re-reject the client

		public bool DecidedToReject { get { return IsReRejected || IsRejected; } } // flag that means customer automation decided to reject or re-reject the client
		public bool DecidedToApprove { get { return IsAutoApproval || IsAutoBankBasedApproval || IsAutoReApproval; } }

		public string AutoRejectReason { get; set; } // auto rejection reason
		public CreditResultStatus? CreditResult { get; set; } // Rejected / Approved / WaitingForDecision
		public Status? UserStatus { get; set; } // Approved / Manual / Rejected
		public SystemDecision? SystemDecision { get; set; } //Approve / Manual / Reject
		public string LoanOfferUnderwriterComment { get; set; } //comment

		public int AutoApproveAmount { get; set; } //Approved / ReApproved amount
		public int RepaymentPeriod { get; set; } //Approve period
		public int BankBasedAutoApproveAmount { get; set; } //Bank based approved amount
		public string DecisionName { get; set; } // Manual / Approval / Re-Approval / Bank Based Approval / Rejection / Re-Rejection

		public DateTime? AppValidFor { get; set; }
		public bool LoanOfferEmailSendingBannedNew { get; set; }
		public int LoanSourceID { get; set; }
		public int LoanTypeID { get; set; }
		public decimal InterestRate { get; set; }
		public decimal SetupFee { get; set; }
		public bool SetupFeeEnabled { get; set; }
		public bool BrokerSetupFeeEnabled { get; set; }
		public decimal ManualSetupFeePercent { get; set; }
		public int ManualSetupFeeAmount { get; set; }
		public bool IsCustomerRepaymentPeriodSelectionAllowed { get; set; }
		public int? DiscountPlanID { get; set; }

	} // class AutoDecisionResponse
} // namespace
