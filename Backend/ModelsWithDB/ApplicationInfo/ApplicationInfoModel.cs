namespace Ezbob.Backend.ModelsWithDB.ApplicationInfo {
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Utils;
	using EZBob.DatabaseLib.Model.Database;

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
		public string CreditResult { get; set; }

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
		public int DiscountPlanId { get; set; }

		[DataMember]
		public int LoanSourceID { get; set; } // Current loan source id.

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
		public decimal RequestedLoanAmount { get; set; }

		[DataMember]
		public int RequestedLoanTerm { get; set; }

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

		[NonTraversable]
		[DataMember]
		public TypeOfBusiness TypeOfBusiness { get; set; }

		[DataMember]
		public bool IsRegulated { get; set; }

		[DataMember]
		public bool IsLimited { get; set; }

		[NonTraversable]
		[DataMember]
		public AutomationOfferModel AutomationOfferModel { get; set; }

		[DataMember]
		public bool SpreadSetupFee { get; set; }

		[DataMember]
		public bool IsMultiBranded { get; set; }
		
		[DataMember]
		public int OriginID { get; set; }

		[DataMember]
		public int NumOfLoans { get; set; }

		[DataMember]
		public bool IsCustomerInEnabledStatus { get; set; }
		//-----------------------------OP ---------------------\\
		[DataMember]
		public int? ProductSubTypeID { get; set; }

		[DataMember]
		public bool UwUpdatedFees { get; set; }

		[NonTraversable]
		[DataMember]
		public decimal? LogicalGlueScore { get; set; }

		[NonTraversable]
		[DataMember]
		public int? GradeID { get; set; }

		[NonTraversable]
		[DataMember]
		public List<I_Product> Products { get; set; }

		[NonTraversable]
		[DataMember]
		public int CurrentProductID { get; set; }

		[NonTraversable]
		[DataMember]
		public List<I_ProductType> ProductTypes { get; set; }

		[NonTraversable]
		[DataMember]
		public int CurrentProductTypeID { get; set; }

		[NonTraversable]
		[DataMember]
		public List<I_ProductSubType> ProductSubTypes { get; set; }

		[NonTraversable]
		[DataMember]
		public I_ProductSubType CurrentProductSubType { get; set; }
		
		[NonTraversable]
		[DataMember]
		public List<I_Grade> Grades { get; set; }

		[NonTraversable]
		[DataMember]
		public List<I_GradeRange> GradeRanges { get; set; }

		[NonTraversable]
		[DataMember]
		public I_GradeRange CurrentGradeRange { get; set; }

		[NonTraversable]
		[DataMember]
		public List<I_FundingType> FundingTypes { get; set; }

		[NonTraversable]
		[DataMember]
		public int CurrentFundingTypeID { get; set; }

		[NonTraversable]
		[DataMember]
		public List<I_SubGrade> SubGrades { get; set; }

		[NonTraversable]
		[DataMember]
		public int? SubGradeID { get; set; }

		[NonTraversable]
		[DataMember]
		public string Origin { get; set; }
	} // class ApplicationInfoModel
} // namespace
