namespace Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions {
	using System;
	using DbConstants;
	using EZBob.DatabaseLib.Model.Database;

	public class AutoDecisionResponse {
		public AutoDecisionResponse() {
			IsAutoBankBasedApproval = false;

			Decision = null;

			AutoRejectReason = null;
			CreditResult = null;
			UserStatus = null;
			SystemDecision = null;
			LoanOfferUnderwriterComment = null;

			AutoApproveAmount = 0;
			RepaymentPeriod = 0;
			BankBasedAutoApproveAmount = 0;
			DecisionName = "Manual";

			AppValidFor = null;
			LoanOfferEmailSendingBannedNew = false;
			LoanSourceID = 0;
			LoanTypeID = 0;
			InterestRate = 0;
			SetupFee = 0;
			IsCustomerRepaymentPeriodSelectionAllowed = false;
			DiscountPlanID = null;
			HasApprovalChance = false;
		} // constructor

		public bool IsAutoBankBasedApproval { get; set; }

		public bool HasAutomaticDecision { get { return Decision != null; } }
		public DecisionActions? Decision { get; set; }

		public int? DecisionCode { get { return Decision == null ? (int?)null : (int)Decision; } }

		public bool IsAutoApproval { get { return Decision == DecisionActions.Approve; } }
		public bool IsAutoReApproval { get { return Decision == DecisionActions.ReApprove; } }
		public bool IsReRejected { get { return Decision == DecisionActions.ReReject; } }
		public bool IsRejected { get { return Decision == DecisionActions.Reject; } }

		public bool DecidedToReject { get { return IsReRejected || IsRejected; } }
		public bool DecidedToApprove { get { return IsAutoApproval || IsAutoBankBasedApproval || IsAutoReApproval; } }

		public string AutoRejectReason { get; set; }
		public CreditResultStatus? CreditResult { get; set; } // Rejected / Approved / WaitingForDecision
		public Status? UserStatus { get; set; } // Approved / Manual / Rejected
		public SystemDecision? SystemDecision { get; set; } // Approve / Manual / Reject
		public string LoanOfferUnderwriterComment { get; set; }

		public int AutoApproveAmount { get; set; }
		public int RepaymentPeriod { get; set; }
		public int BankBasedAutoApproveAmount { get; set; }
		public string DecisionName { get; set; } // Manual/Approval/Re-Approval/Bank Based Approval/Rejection/Re-Rejection

		public DateTime? AppValidFor { get; set; }
		public bool LoanOfferEmailSendingBannedNew { get; set; }
		public int LoanSourceID { get; set; }
		public int LoanTypeID { get; set; }
		public decimal InterestRate { get; set; }
		public decimal SetupFee { get; set; }
		public bool IsCustomerRepaymentPeriodSelectionAllowed { get; set; }
		public int? DiscountPlanID { get; set; }
		public bool HasApprovalChance { get; set; }
	} // class AutoDecisionResponse
} // namespace
