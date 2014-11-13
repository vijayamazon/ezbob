namespace EzBob.Backend.Strategies.MainStrategy.AutoDecisions
{
	using System;
	using Ezbob.Backend.Models;

	public class AutoDecisionResponse
	{
		public bool IsReRejected { get; set; }
		public string AutoRejectReason { get; set; }
		public string CreditResult { get; set; }
		public string UserStatus { get; set; }
		public string SystemDecision { get; set; }
		public string LoanOfferUnderwriterComment { get; set; }
		public DateTime? AppValidFor { get; set; }
		public bool LoanOfferEmailSendingBannedNew { get; set; }
		public bool IsAutoApproval { get; set; }
		public bool IsAutoBankBasedApproval { get; set; }
		public int AutoApproveAmount { get; set; }
		public int RepaymentPeriod { get; set; }
		public int BankBasedAutoApproveAmount { get; set; }
		public RejectionModel RejectionModel { get; set; }
		public string DecisionName { get; set; }
		public bool IsEu { get; set; }
		public int LoanTypeId { get; set; }
		public decimal InterestRate { get; set; }
		public decimal SetupFee { get; set; }
	}
}
