using System;
using Iesi.Collections.Generic;

namespace EZBob.DatabaseLib.Model.Database
{
    public class MP_YodleeOrderItem
    {
        public MP_YodleeOrderItem()
        {
            OrderItemBankTransactions = new HashedSet<MP_YodleeOrderItemBankTransaction>();
        }

        public virtual int Id { get; set; }

        public virtual MP_YodleeOrder Order { get; set; }

        public virtual Iesi.Collections.Generic.ISet<MP_YodleeOrderItemBankTransaction> OrderItemBankTransactions { get; set; }

        public long? isSeidFromDataSource { get; set; }
        public bool isSeidFromDataSourceSpecified { get; set; }
        public long? isSeidMod { get; set; }
        public bool isSeidModSpecified { get; set; }
        public long? acctTypeId { get; set; }
        public bool acctTypeIdSpecified { get; set; }
        public string acctType { get; set; }
        public string localizedAcctType { get; set; }
        public string srcElementId { get; set; }
        public long? individualInformationId { get; set; }
        public bool individualInformationIdSpecified { get; set; }
        public long? bankAccountId { get; set; }
        public bool bankAccountIdSpecified { get; set; }
        public string customName { get; set; }
        public string customDescription { get; set; }
        public long? isDeleted { get; set; }
        public bool isDeletedSpecified { get; set; }
        public long? lastUpdated { get; set; }
        public bool lastUpdatedSpecified { get; set; }
        public long? hasDetails { get; set; }
        public bool hasDetailsSpecified { get; set; }
        public double? interestRate { get; set; }
        public bool interestRateSpecified { get; set; }
        public string accountNumber { get; set; }
        public string link { get; set; }
        public string accountHolder { get; set; }
        public DateTime? tranListToDate { get; set; }
        public DateTime? tranListFromDate { get; set; }
        public double? availableBalance { get; set; }
        public double? currentBalance { get; set; }
        public double? interestEarnedYtd { get; set; }
        public double? prevYrInterest { get; set; }
        public double? overdraftProtection { get; set; }
        public string term { get; set; }
        public string accountName { get; set; }
        public double? annualPercentYield { get; set; }
        public bool annualPercentYieldSpecified { get; set; }
        public string routingNumber { get; set; }
        public DateTime? maturityDate { get; set; }
        public DateTime? asOfDate { get; set; }
        //public object[] bankStatements { get; set; }
        //public IndividualInformation individualInformation { get; set; }
        public string accountNicknameAtSrcSite { get; set; }
        public long? isPaperlessStmtOn { get; set; }
        public bool isPaperlessStmtOnSpecified { get; set; }
        //public SiteAccountStatus? siteAccountStatus { get; set; }
        public bool siteAccountStatusSpecified { get; set; }
        public long? created { get; set; }
        public bool createdSpecified { get; set; }
        public string nomineeName { get; set; }
        public string secondaryAccountHolderName { get; set; }
        public DateTime? accountOpenDate { get; set; }
        public DateTime? accountCloseDate { get; set; }
        public double? maturityAmount { get; set; }
        public double? taxesWithheldYtd { get; set; }
        public double? taxesPaidYtd { get; set; }
        public double? budgetBalance { get; set; }
        public double? straightBalance { get; set; }
        //public AccountClassification? accountClassification { get; set; }
        public bool accountClassificationSpecified { get; set; }


    }
}