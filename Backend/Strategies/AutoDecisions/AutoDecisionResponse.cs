namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;

	public class AutoDecisionResponse
	{
		public bool IsReRejected { get; set; }
		public string AutoRejectReason { get; set; }
		public string CreditResult { get; set; }
		public string UserStatus { get; set; }
		public string SystemDecision { get; set; }
		public string LoanOfferUnderwriterComment { get; set; }
		public double LoanOfferOfferValidDays { get; set; } // TODO: remove it, its usage is fake
		public DateTime? AppApplyForLoan { get; set; } // TODO: remove it, its usage is fake (always null)
		public DateTime? AppValidFor { get; set; }
		public bool LoanOfferEmailSendingBannedNew { get; set; }
		public bool IsAutoApproval { get; set; }
		public bool IsAutoBankBasedApproval { get; set; }
		public int AutoApproveAmount { get; set; }
		public decimal PayPalTotalSumOfOrders3M { get; set; }
		public decimal PayPalTotalSumOfOrders1Y { get; set; }
		public int PayPalNumberOfStores { get; set; }
	}
}
