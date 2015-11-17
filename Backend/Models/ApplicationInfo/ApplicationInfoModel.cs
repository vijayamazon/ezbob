namespace Ezbob.Backend.Models.ApplicationInfo {
	using System.Runtime.Serialization;

	[DataContract]
	public class ApplicationInfoModel {
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public int CustomerId { get; set; }

		[DataMember]
		public string CustomerName { get; set; }

		[DataMember]
		public string CustomerRefNum { get; set; }

		[DataMember]
		public string SystemDecision { get; set; }

		[DataMember]
		public decimal SystemCalculatedAmount { get; set; }

		[DataMember]
		public decimal OfferedCreditLine { get; set; }

		[DataMember]
		public decimal BorrowedAmount { get; set; }

		[DataMember]
		public decimal AvailableAmount { get; set; }

		[DataMember]
		public int RepaymentPeriod { get; set; }

		[DataMember]
		public string StartingFromDate { get; set; }

		[DataMember]
		public decimal InterestRate { get; set; }

		[DataMember]
		public string OfferValidateUntil { get; set; }

		[DataMember]
		public string Details { get; set; }

		[DataMember]
		public bool Editable { get; set; }

		[DataMember]
		public long CashRequestId { get; set; }

		[DataMember]
		public byte[] CashRequestTimestamp { get; set; }

		[DataMember]
		public decimal? ManualSetupFeePercent { get; set; }

		[DataMember]
		public decimal? BrokerSetupFeePercent { get; set; }

		[DataMember]
		public decimal TotalSetupFee { get; set; }

		[DataMember]
		public decimal TotalSetupFeePercent { get; set; }

		[DataMember]
		public decimal BrokerSetupFee { get; set; }

		[DataMember]
		public decimal BrokerSetupFeeActualPercent { get; set; }

		[DataMember]
		public decimal SetupFee { get; set; }

		[DataMember]
		public decimal SetupFeeActualPercent { get; set; }

		[DataMember]
		public bool IsTest { get; set; }

		[DataMember]
		public bool? IsOffline { get; set; }

		[DataMember]
		public bool HasYodlee { get; set; }

		[DataMember]
		public bool IsAvoid { get; set; }

		[DataMember]
		public bool AllowSendingEmail { get; set; }

		[DataMember]
		public string LoanType { get; set; }

		[DataMember]
		public bool OfferExpired { get; set; }

		[DataMember]
		public bool IsModified { get; set; }

		[DataMember]
		public int IsLoanTypeSelectionAllowed { get; set; }

		[DataMember]
		public bool IsCustomerRepaymentPeriodSelectionAllowed { get; set; }

		[DataMember]
		public int LoanTypeId { get; set; } // Current loan type id.

		[DataMember]
		public LoanTypeModel[] LoanTypes { get; set; } // All the loan types.

		[DataMember]
		public string Reason { get; set; }

		[DataMember]
		public int OfferValidForHours { get; set; }

		[DataMember]
		public DiscountPlanModel[] DiscountPlans { get; set; }

		[DataMember]
		public string DiscountPlan { get; set; }

		[DataMember]
		public string DiscountPlanPercents { get; set; }

		[DataMember]
		public int DiscountPlanId { get; set; }

		[DataMember]
		public int LoanSourceID { get; set; } //current loan source id

		[DataMember]
		public string LoanSource { get; set; } //current loan source name

		[DataMember]
		public LoanSourceModel[] AllLoanSources { get; set; } //all loan sources

		[DataMember]
		public string AMLResult { get; set; }

		[DataMember]
		public bool SkipPopupForApprovalWithoutAML { get; set; }

		[DataMember]
		public int EmployeeCount { get; set; }

		[DataMember]
		public decimal AnnualTurnover { get; set; }

		[DataMember]
		public int CustomerReasonType { get; set; }

		[DataMember]
		public string CustomerReason { get; set; }

		[DataMember]
		public decimal Turnover { get; set; }

		[DataMember]
		public decimal FreeCashFlow { get; set; }

		[DataMember]
		public decimal ValueAdded { get; set; }

		[DataMember]
		public double Apr { get; set; }

		[DataMember]
		public decimal Air { get; set; }

		[DataMember]
		public decimal RealCost { get; set; }

		[DataMember]
		public SuggestedAmountModel[] SuggestedAmounts { get; set; }

		[DataMember]
		public int TypeOfBusiness { get; set; }

		[DataMember]
		public AutomationOfferModel AutomationOfferModel { get; set; }

		[DataMember]
		public bool SpreadSetupFee { get; set; }
	} // class ApplicationInfoModel
} // namespace
