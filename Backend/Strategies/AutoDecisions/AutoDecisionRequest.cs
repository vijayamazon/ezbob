﻿namespace EzBob.Backend.Strategies.AutoDecisions
{
	public class AutoDecisionRequest
	{
		public int LoanOfferReApprovalFullAmount { get; set; }
		public int LoanOfferReApprovalRemainingAmount { get; set; }
		public int LoanOfferReApprovalFullAmountOld { get; set; }
		public int LoanOfferReApprovalRemainingAmountOld { get; set; }
		public int CustomerId { get; set; }
		public bool EnableAutomaticApproval { get; set; }
		public double InitialExperianConsumerScore { get; set; }
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
	}
}
