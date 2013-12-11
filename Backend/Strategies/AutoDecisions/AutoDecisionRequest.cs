namespace EzBob.Backend.Strategies.AutoDecisions
{
	using System;

	public class AutoDecisionRequest
	{
		// TODO: try to reduce the number of properties here as much as possible
		public int LoanOffer_ReApprovalFullAmount { get; set; }
		public int LoanOffer_ReApprovalRemainingAmount { get; set; }
		public int LoanOffer_ReApprovalFullAmountOld { get; set; }
		public int LoanOffer_ReApprovalRemainingAmountOld { get; set; }
		public int CustomerId { get; set; }
		public bool EnableAutomaticApproval { get; set; }
		public double Inintial_ExperianConsumerScore { get; set; }
		public double MarketplaceSeniorityDays { get; set; }
		public bool EnableAutomaticReApproval { get; set; }
		public int MinExperianScore { get; set; }
		public int OfferedCreditLine { get; set; }
		public int LowTotalAnnualTurnover { get; set; }
		public int LowTotalThreeMonthTurnover { get; set; }
		public bool EnableAutomaticRejection { get; set; }
		public bool EnableAutomaticReRejection { get; set; }
		public double TotalSumOfOrders1YTotal { get; set; }
		public double TotalSumOfOrders3MTotal { get; set; }

		public bool IsReRejected { get; set; }
		public int ModelLoanOffer { get; set; }
		public string LoanOffer_UnderwriterComment { get; set; }
		public double LoanOffer_OfferValidDays { get; set; }
		public DateTime? App_ApplyForLoan { get; set; }
		public DateTime App_ValidFor { get; set; }
		public bool LoanOffer_EmailSendingBanned_new { get; set; }
		public bool IsAutoApproval { get; set; }
	}
}
