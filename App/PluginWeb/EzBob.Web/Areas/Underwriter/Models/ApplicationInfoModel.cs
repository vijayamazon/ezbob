﻿namespace EzBob.Web.Areas.Underwriter.Models
{
    public class ApplicationInfoModel
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string SystemDecision { get; set; }
        public decimal SystemCalculatedAmount { get; set; }
        public decimal OfferedCreditLine { get; set; }
        public decimal BorrowedAmount { get; set; }
        public decimal AvaliableAmount { get; set; }
        public int RepaymentPerion { get; set; }
        public string StartingFromDate { get; set; }
        public decimal InterestRate { get; set; }
		public string OfferValidateUntil { get; set; }
		public string FundsAvaliable { get; set; }
		public string FundsAvailableUnderLimitClass { get; set; }
        public string FundsReserved { get; set; }
        //public string Status { get; set; }
        public string Details { get; set; }
        public bool Editable { get; set; }
        public long CashRequestId { get; set; }
        public bool UseSetupFee { get; set; }
        public decimal SetupFee { get; set; }
        public bool IsTest { get; set; }
        public bool IsAvoid { get; set; }
        public bool AllowSendingEmail { get; set; }

        public string LoanType { get; set; }

        public bool OfferExpired { get; set; }

        public bool IsModified { get; set; }

        public LoanTypesModel[] LoanTypes { get; set; }

        public int LoanTypeId { get; set; }

        public string Reason { get; set; }

        public int IsLoanTypeSelectionAllowed { get; set; }

        public DiscountPlanModel[] DiscountPlans { get; set; }
        public string DiscountPlan { get; set; }
        public string DiscountPlanPercents { get; set; }
        public int DiscountPlanId { get; set; }
    }
}