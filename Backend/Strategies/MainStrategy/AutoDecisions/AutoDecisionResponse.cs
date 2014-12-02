namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions
{
	using System;
	using Ezbob.Backend.Models;

	public class AutoDecisionResponse
	{
		public bool DecidedToReject { get; set; } // flag that means customer automation decided to reject or re-reject the client
		public bool IsAutoApproval { get; set; } // flag that means automation decided to approve customer
		public bool IsAutoReApproval { get; set; } // flag that means automation decided to re-approve customer
		public bool IsAutoBankBasedApproval { get; set; } //flag that means automation decided to give bank based approval
		public bool IsReRejected { get; set; } // flag that means customer automation decided to re-reject the client

		public string AutoRejectReason { get; set; } // auto rejection reason
		public string CreditResult { get; set; } // Rejected / Approved / WaitingForDecision
		public string UserStatus { get; set; } // Approved / Manual / Rejected
		public string SystemDecision { get; set; } //Approve / Manual / Reject
		public string LoanOfferUnderwriterComment { get; set; } //comment
		
		public int AutoApproveAmount { get; set; } //Approved amount
		public int RepaymentPeriod { get; set; } //Approve period
		public int BankBasedAutoApproveAmount { get; set; } //Bank based approved amount
		public RejectionModel RejectionModel { get; set; } // Rejection fields (todo retire)
		public string DecisionName { get; set; } // Manual / Approval / Re-Approval / Bank Based Approval / Rejection / Re-Rejection

		public DateTime? AppValidFor { get; set; }
		public bool LoanOfferEmailSendingBannedNew { get; set; }
		public bool IsEu { get; set; }
		public int LoanTypeId { get; set; }
		public decimal InterestRate { get; set; }
		public decimal SetupFee { get; set; }
	}
}
