namespace Ezbob.Backend.Models.ApplicationInfo {
	using System.Linq;
	using System.Runtime.Serialization;
	using Ezbob.Utils;

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
		public bool Editable { get; set; }

		[DataMember]
		public long CashRequestId { get; set; }

		public string CashRequestRowVersion {
			get {
				return CashRequestTimestamp == null
					? string.Empty
					: string.Join("", CashRequestTimestamp.Select(b => b.ToString("x2")));
			} // get
		} // CashRequestRowVersion

		[DataMember]
		public byte[] CashRequestTimestamp { get; set; }

		[DataMember]
		public decimal? ManualSetupFeePercent { get; set; }

		[DataMember]
		public decimal? BrokerSetupFeePercent { get; set; }

		[DataMember]
		public decimal TotalSetupFee { get; set; }

		[DataMember]
		public decimal BrokerSetupFee { get; set; }

		public decimal TotalSetupFeePercent {
			get { return OfferedCreditLine == 0 ? 0 : TotalSetupFee / OfferedCreditLine; }
		} // TotalSetupFeePercent

		public decimal BrokerSetupFeeActualPercent {
			get { return OfferedCreditLine == 0 ? 0 : BrokerSetupFee / OfferedCreditLine; }
		} // BrokerSetupFeeActualPercent

		public decimal SetupFee {
			get { return TotalSetupFee - BrokerSetupFee; }
		} // SetupFee

		public decimal SetupFeeActualPercent {
			get { return OfferedCreditLine == 0 ? 0 : SetupFee / OfferedCreditLine; }
		} // SetupFeeActualPercent

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

		[NonTraversable]
		[DataMember]
		public LoanTypeModel[] LoanTypes { get; set; } // All the loan types.

		[DataMember]
		public string Reason { get; set; }

		[DataMember]
		public int OfferValidForHours { get; set; }

		[NonTraversable]
		[DataMember]
		public DiscountPlanModel[] DiscountPlans { get; set; }

		[DataMember]
		public string DiscountPlan { get; set; }

		[DataMember]
		public string DiscountPlanPercents { get; set; }

		[DataMember]
		public int DiscountPlanId { get; set; }

		[DataMember]
		public int LoanSourceID { get; set; } // Current loan source id.

		[DataMember]
		public string LoanSource { get; set; } // Current loan source name.

		[NonTraversable]
		[DataMember]
		public LoanSourceModel[] AllLoanSources { get; set; } // All loan sources.

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

		public decimal Air {
			get {
				return
					(InterestRate * 100m * 12m + (
						RepaymentPeriod == 0
							? 0
							: (12m / RepaymentPeriod * TotalSetupFeePercent * 100)
					)) / 100m;
			} // get
		} // Air

		[DataMember]
		public decimal RealCost { get; set; }

		[NonTraversable]
		[DataMember]
		public SuggestedAmountModel[] SuggestedAmounts { get; set; }

		[DataMember]
		public int TypeOfBusiness { get; set; }

		[NonTraversable]
		[DataMember]
		public AutomationOfferModel AutomationOfferModel { get; set; }

		[DataMember]
		public bool SpreadSetupFee { get; set; }

		[DataMember]
		public bool IsMultiBranded { get; set; }
	} // class ApplicationInfoModel
} // namespace
