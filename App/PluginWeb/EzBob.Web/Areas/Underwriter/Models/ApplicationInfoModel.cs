namespace EzBob.Web.Areas.Underwriter.Models {
	public class ApplicationInfoModel {
		public int Id { get; set; }
		public int CustomerId { get; set; }
		public string CustomerName { get; set; }
		public string CustomerRefNum { get; set; }
		public string SystemDecision { get; set; }
		public decimal SystemCalculatedAmount { get; set; }
		public decimal OfferedCreditLine { get; set; }
		public decimal BorrowedAmount { get; set; }
		public decimal AvaliableAmount { get; set; }
		public int RepaymentPerion { get; set; }
		public string StartingFromDate { get; set; }
		public decimal InterestRate { get; set; }
		public string OfferValidateUntil { get; set; }
		//public string Status { get; set; }
		public string Details { get; set; }
		public bool Editable { get; set; }
		public long CashRequestId { get; set; }

		public decimal? ManualSetupFeePercent { get; set; }
		public decimal? BrokerSetupFeePercent { get; set; }

		public decimal TotalSetupFee { get; set; }
		public decimal TotalSetupFeePercent { get; set; }
		public decimal BrokerSetupFee { get; set; }
		public decimal BrokerSetupFeeActualPercent { get; set; }
		public decimal SetupFee { get; set; }
		public decimal SetupFeeActualPercent { get; set; }

		public bool IsTest { get; set; }

		public bool? IsOffline { get; set; }
		public bool HasYodlee { get; set; }

		public bool IsAvoid { get; set; }
		public bool AllowSendingEmail { get; set; }

		public string LoanType { get; set; }
		public bool OfferExpired { get; set; }

		public bool IsModified { get; set; }

		public int IsLoanTypeSelectionAllowed { get; set; }
		public bool IsCustomerRepaymentPeriodSelectionAllowed { get; set; }

		public int LoanTypeId { get; set; } //current loan type id
		
		public LoanTypesModel[] LoanTypes { get; set; } //all loan types

		public string Reason { get; set; }

		public int OfferValidForHours { get; set; }

		public DiscountPlanModel[] DiscountPlans { get; set; }
		public string DiscountPlan { get; set; }
		public string DiscountPlanPercents { get; set; }
		public int DiscountPlanId { get; set; }

		public int LoanSourceID { get; set; } //current loan source id
		public string LoanSource { get; set; } //current loan source name
		public LoanSourceModel[] AllLoanSources { get; set; } //all loan sources

		public string AMLResult { get; set; }
		public bool SkipPopupForApprovalWithoutAML { get; set; }

		public int EmployeeCount { get; set; }
		public decimal AnnualTurnover { get; set; }
		public int CustomerReasonType { get; set; }
		public string CustomerReason { get; set; }

		public decimal Turnover { get; set; }
		public decimal FreeCashFlow { get; set; }
		public decimal ValueAdded { get; set; }

		public double Apr { get; set; }
		public decimal Air { get; set; }
		public decimal RealCost { get; set; }

		public SuggestedAmountModel[] SuggestedAmounts { get; set; }
		public int TypeOfBusiness { get; set; }

		public AutomationOfferModel AutomationOfferModel { get; set; }

		public bool SpreadSetupFee { get; set; }
	} // class ApplicationInfoModel

	public class AutomationOfferModel {
		public int Amount { get; set; }
		public decimal InterestRate { get; set; }
		public int RepaymentPeriod { get; set; }
		public decimal SetupFeePercent { get; set; }
		public decimal SetupFeeAmount { get; set; }
	} // class AutomationOfferModel
} // namespace
