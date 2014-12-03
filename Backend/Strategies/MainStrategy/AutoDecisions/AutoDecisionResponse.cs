﻿namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions {
	using System;
	using DbConstants;

	public class AutoDecisionResponse {
		public bool IsAutoBankBasedApproval { get; set; } //flag that means automation decided to give bank based approval

		public DecisionActions? Decision { get; set; }

		public int? DecisionCode { get { return Decision == null ? (int?)null : (int)Decision; } }

		public bool IsAutoApproval { get { return Decision == DecisionActions.Approve; } }
		public bool IsAutoReApproval { get { return Decision == DecisionActions.ReApprove; } }
		public bool IsReRejected { get { return Decision == DecisionActions.ReReject; } } // flag that means customer automation decided to re-reject the client
		public bool IsRejected { get { return Decision == DecisionActions.Reject; } } // flag that means customer automation decided to re-reject the client
		public bool DecidedToReject { get { return IsReRejected || IsRejected; } } // flag that means customer automation decided to reject or re-reject the client

		public string AutoRejectReason { get; set; } // auto rejection reason
		public string CreditResult { get; set; } // Rejected / Approved / WaitingForDecision
		public string UserStatus { get; set; } // Approved / Manual / Rejected
		public string SystemDecision { get; set; } //Approve / Manual / Reject
		public string LoanOfferUnderwriterComment { get; set; } //comment

		public int AutoApproveAmount { get; set; } //Approved amount
		public int RepaymentPeriod { get; set; } //Approve period
		public int BankBasedAutoApproveAmount { get; set; } //Bank based approved amount
		public string DecisionName { get; set; } // Manual / Approval / Re-Approval / Bank Based Approval / Rejection / Re-Rejection

		public DateTime? AppValidFor { get; set; }
		public bool LoanOfferEmailSendingBannedNew { get; set; }
		public bool IsEu { get; set; }
		public int LoanTypeId { get; set; }
		public decimal InterestRate { get; set; }
		public decimal SetupFee { get; set; }
	} // class AutoDecisionResponse
} // namespace
