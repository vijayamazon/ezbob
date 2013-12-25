namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;

	public class AutoDecisionResponse
	{
		public AutoDecisionResponse(AutoDecisionRequest request)
		{
			IsReRejected = request.IsReRejected;
			ModelLoanOffer = request.ModelLoanOffer;
		}

		public bool IsReRejected { get; set; }
		public string AutoRejectReason { get; set; }
		public string CreditResult { get; set; }
		public string UserStatus { get; set; }
		public string SystemDecision { get; set; }
		public int ModelLoanOffer { get; set; }
		public string LoanOfferUnderwriterComment { get; set; }
		public double LoanOfferOfferValidDays { get; set; }
		public DateTime? AppApplyForLoan { get; set; }
		public DateTime AppValidFor { get; set; }
		public bool LoanOfferEmailSendingBannedNew { get; set; }
		public bool IsAutoApproval { get; set; }
		public int AutoApproveAmount { get; set; }
		public decimal PayPalTotalSumOfOrders3M { get; set; }
		public decimal PayPalTotalSumOfOrders1Y { get; set; }
		public int PayPalNumberOfStores { get; set; }
	}
}
