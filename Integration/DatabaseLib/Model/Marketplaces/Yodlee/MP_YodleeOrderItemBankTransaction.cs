namespace EZBob.DatabaseLib.Model.Database
{
    using System;

    public class MP_YodleeOrderItemBankTransaction
    {
        public virtual int Id { get; set; }
        public virtual int OrderItemId { get; set; }

        public long? isSeidFromDataSource { get; set; }
        public bool isSeidFromDataSourceSpecified { get; set; }
        public long? isSeidMod { get; set; }
        public bool isSeidModSpecified { get; set; }
        public string srcElementId { get; set; }
        public long? transactionTypeId { get; set; }
        public bool transactionTypeIdSpecified { get; set; }
        public string transactionType { get; set; }
        public string localizedTransactionType { get; set; }
        public long? transactionStatusId { get; set; }
        public bool transactionStatusIdSpecified { get; set; }
        public string transactionStatus { get; set; }
        public string localizedTransactionStatus { get; set; }
        public long? transactionBaseTypeId { get; set; }
        public bool transactionBaseTypeIdSpecified { get; set; }
        public string transactionBaseType { get; set; }
        public string localizedTransactionBaseType { get; set; }
        public long? categoryId { get; set; }
        public bool categoryIdSpecified { get; set; }
        public long? bankTransactionId { get; set; }
        public bool bankTransactionIdSpecified { get; set; }
        public long? bankAccountId { get; set; }
        public bool bankAccountIdSpecified { get; set; }
        public long? bankStatementId { get; set; }
        public bool bankStatementIdSpecified { get; set; }
        public long? isDeleted { get; set; }
        public bool isDeletedSpecified { get; set; }
        public long? lastUpdated { get; set; }
        public bool lastUpdatedSpecified { get; set; }
        public long? hasDetails { get; set; }
        public bool hasDetailsSpecified { get; set; }
        public string transactionId { get; set; }
        public string transactionCategoryId { get; set; }
        public string siteCategoryType { get; set; }
        public string siteCategory { get; set; }
        public string classUpdationSource { get; set; }
        public string lastCategorised { get; set; }
        public DateTime? transactionDate { get; set; }
        public long? isReimbursable { get; set; }
        public bool isReimbursableSpecified { get; set; }
        public string mcCode { get; set; }
        public long? prevLastCategorised { get; set; }
        public bool prevLastCategorisedSpecified { get; set; }
        public string naicsCode { get; set; }
        public double? runningBalance { get; set; }
        public string userDescription { get; set; }
        public long? customCategoryId { get; set; }
        public bool customCategoryIdSpecified { get; set; }
        public string memo { get; set; }
        public long? parentId { get; set; }
        public bool parentIdSpecified { get; set; }
        public long? isOlbUserDesc { get; set; }
        public bool isOlbUserDescSpecified { get; set; }
        public string categorisationSourceId { get; set; }
        public string plainTextDescription { get; set; }
        public string splitType { get; set; }
        public long? categoryLevelId { get; set; }
        public bool categoryLevelIdSpecified { get; set; }
        public double? calcRunningBalance { get; set; }
        public string category { get; set; }
        public string link { get; set; }
        public DateTime? postDate { get; set; }
        public long? prevTransactionCategoryId { get; set; }
        public bool prevTransactionCategoryIdSpecified { get; set; }
        public long? isBusinessExpense { get; set; }
        public bool isBusinessExpenseSpecified { get; set; }
        public long? descriptionViewPref { get; set; }
        public bool descriptionViewPrefSpecified { get; set; }
        public long? prevCategorisationSourceId { get; set; }
        public bool prevCategorisationSourceIdSpecified { get; set; }
        public double? transactionAmount { get; set; }
        public long? transactionPostingOrder { get; set; }
        public bool transactionPostingOrderSpecified { get; set; }
        public string checkNumber { get; set; }
        public string description { get; set; }
        public long? isTaxDeductible { get; set; }
        public bool isTaxDeductibleSpecified { get; set; }
        public long? isMedicalExpense { get; set; }
        public bool isMedicalExpenseSpecified { get; set; }
        public string categorizationKeyword { get; set; }
        public string sourceTransactionType { get; set; }
    }
}