using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EzEverlineLoanStatusCheckTool {
    public enum EverlineLoanStatus
    {
        Error,
        DoesNotExist,
        ExistsWithNoLiveLoan,
        ExistsWithCurrentLiveLoan
    }

    public class EverlineLoginLoanCheckerResult
    {
        public EverlineLoanStatus status { get; set; }
        public string Message { get; set; }
    }

    public class EverlineBusinessOrganisationDetails
    {
        [JsonProperty("loans")]
        public IEnumerable<EverlineLoanApplication> LoanApplications { get; set; }

        [JsonProperty("loansCount")]
        public int LoanApplicationsCount { get; set; }

        [JsonProperty("firstLoanFundedDate")]
        public DateTime? FirstLoanApplicationFundedDate { get; set; }
    }

    public class EverlineLoanApplication
    {
        [JsonProperty("loanId")]
        public Guid LoanId { get; set; }

        [JsonProperty("loanAmount")]
        public decimal LoanAmount { get; set; }

        [JsonProperty("term")]
        public int Term { get; set; }

        [JsonProperty("fundedOn")]
        public DateTime? FundedOn { get; set; }

        [JsonProperty("closedOn")]
        public DateTime? ClosedOn { get; set; }

        [JsonProperty("daysClosedLateCount")]
        public int? DaysClosedLateCount { get; set; }

        [JsonProperty("defaultChargesCount")]
        public int? DefaultChargesCount { get; set; }

        [JsonProperty("gaugeScore")]
        public int? GaugeScore { get; set; }

        [JsonProperty("augurScore")]
        public int? AugurScore { get; set; }

        [JsonProperty("balanceDetails")]
        public EverlineBalanceDetails BalanceDetails { get; set; }

        [JsonProperty("transactions")]
        public IEnumerable<EverlineLoanApplicationTransaction> Transactions { get; set; }
    }

    public class EverlineLoanApplicationTransaction
    {
        [JsonProperty("transactionId")]
        public Guid TransactionId { get; set; }

        [JsonProperty("postedOn")]
        public DateTime? PostedOn { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("createdOn")]
        public DateTime? CreatedOn { get; set; }

        [JsonProperty("transationType")]
        public EverlineTransactionType TransactionType { get; set; }
    }

    public class EverlineBalanceDetails
    {
        [JsonProperty("totalOutstandingBalance")]
        public decimal? TotalOutstandingBalance { get; set; }
    }

    public enum EverlineTransactionType
    {
        CashAdvance = 1,
        Fee = 2,
        LoanExtensionFee = 3,
        Interest = 4,
        DefaultCharge = 5,
        CardPayment = 6,
        WriteOff = 7,
        DebtSale = 8,
        DebtSaleProceeds = 9,
        InterestWriteOff = 10,
        AgencyFee = 11,
        Adjustment = 12,
        ArrearsLetterCharge = 13,
        OutsourcedAgencyPayment = 14,
        Refund = 15,
        Chargeback = 16,
        Cheque = 17,
        DirectBankPayment = 18,
        SuspendInterestAccrual = 19,
        ResumeInterestAccrual = 20,
        V2MigratedInsurance = 21,
        V2MigratedWriteOff = 22,
        V2MigratedCharge = 23,
        V2MigratedPayment = 24,
        V2Migratedadvance = 25,
        ServiceFee = 26,
        InitiationFee = 27,
        InterestRate = 28,
        InterestRateFee = 29,
        InterestAdjustment = 30,
        RegularCardPayment = 31,
        Dispute = 32,
        DirectDeposit = 33,
        InterestReversal = 34,
        ServiceFeeWriteOff = 35,
        ServiceFeeReversal = 36,
        InitiationFeeWriteOff = 37,
        InitiationFeeReversal = 38,
        CashAdvanceReversal = 39,
        FeeReversal = 40,
        BatchedDcaReceipt = 41,
        BatchedDmcReceipt = 42,
        LoanExtensionCardPayment = 43,
        SuspendServiceFee = 44,
        ResumeServiceFee = 45,
        ProductCommissionFee = 46,
        MerchantPaidChargeback = 49,
        MarketingIncentive = 50,
        SuspendDefaultCharge = 51,
        ResumeDefaultCharge = 52,
        ArrearsCommissionFee = 53,
        SuspendTransmissionFee = 54,
        Compensation = 55,
        SettlementOfferWriteOff = 56,
        FraudWriteOff = 57,
        DebtSaleWriteOff = 58,
        LoanExtensionPayment = 59,
    }
}
