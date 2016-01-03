namespace EzBobModels.Yodlee {
    using System;

    /// <summary>
    /// Yodlee v9 model. It's actually an account info
    /// </summary>
    public class YodleeOrderItem {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int? isSeidFromDataSource { get; set; }
        public bool? isSeidFromDataSourceSpecified { get; set; }
        public int? isSeidMod { get; set; }
        public int? isSeidModSpecified { get; set; }
        public int? acctTypeId { get; set; }
        public bool? acctTypeIdSpecified { get; set; }
        public string acctType { get; set; }
        public string localizedAcctType { get; set; }
        public string srcElementId { get; set; }
        public int? individualInformationId { get; set; }
        public int? individualInformationIdSpecified { get; set; }
        public int? bankAccountId { get; set; }
        public bool? bankAccountIdSpecified { get; set; }
        public string customName { get; set; }
        public string customDescription { get; set; }
        public int? isDeleted { get; set; }
        public bool? isDeletedSpecified { get; set; }
        public int? lastUpdated { get; set; }
        public bool? lastUpdatedSpecified { get; set; }
        public int? hasDetails { get; set; }
        public bool? hasDetailsSpecified { get; set; }
        public decimal? interestRate { get; set; }
        public bool interestRateSpecified { get; set; }
        public string accountNumber { get; set; }
        public string link { get; set; }
        public string accountHolder { get; set; }
        public DateTime? tranListToDate { get; set; }
        public DateTime? tranListFromDate { get; set; }
        public decimal? availableBalance { get; set; }
        public string availableBalanceCurrency { get; set; }
        public decimal? currentBalance { get; set; }
        public string currentBalanceCurrency { get; set; }
        public decimal? interestEarnedYtd { get; set; }
        public string interestEarnedYtdCurrency { get; set; }
        public decimal? prevYrInterest { get; set; }
        public string prevYrInterestCurrency { get; set; }
        public decimal? overdraftProtection { get; set; }
        public string overdraftProtectionCurrency { get; set; }
        public string term { get; set; }
        public string accountName { get; set; }
        public decimal? annualPercentYield { get; set; }
        public bool? annualPercentYieldSpecified { get; set; }
        public string routingNumber { get; set; }
        public DateTime? maturityDate { get; set; }
        public DateTime? asOfDate { get; set; }
        public string accountNicknameAtSrcSite { get; set; }
        public int? isPaperlessStmtOn { get; set; }
        public bool? isPaperlessStmtOnSpecified { get; set; }
        public bool? siteAccountStatusSpecified { get; set; }
        public int? created { get; set; }
        public bool? createdSpecified { get; set; }
        public string nomineeName { get; set; }
        public string secondaryAccountHolderName { get; set; }
        public DateTime? accountOpenDate { get; set; }
        public DateTime? accountCloseDate { get; set; }
        public decimal? maturityAmount { get; set; }
        public string maturityAmountCurrency { get; set; }
        public decimal? taxesWithheldYtd { get; set; }
        public string taxesWithheldYtdCurrency { get; set; }
        public decimal? taxesPaidYtd { get; set; }
        public string taxesPaidYtdCurrency { get; set; }
        public decimal? budgetBalance { get; set; }
        public string budgetBalanceCurrency { get; set; }
        public decimal? straightBalance { get; set; }
        public string straightBalanceCurrency { get; set; }
        public bool? accountClassificationSpecified { get; set; }
        public string siteAccountStatus { get; set; }
        public string accountClassification { get; set; }
        public long? itemAccountId { get; set; }
    }
}