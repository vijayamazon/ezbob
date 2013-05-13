namespace EZBob.DatabaseLib.Model.Database
{
    using System;

    public class MP_YodleeOrderItemBankTransaction
    {
        public virtual int Id { get; set; }
        public virtual int OrderItemId { get; set; }

        public virtual long? isSeidFromDataSource { get; set; }
        public virtual bool isSeidFromDataSourceSpecified { get; set; }
        public virtual long? isSeidMod { get; set; }
        public virtual bool isSeidModSpecified { get; set; }
        public virtual string srcElementId { get; set; }
        public virtual long? transactionTypeId { get; set; }
        public virtual bool transactionTypeIdSpecified { get; set; }
        public virtual string transactionType { get; set; }
        public virtual string localizedTransactionType { get; set; }
        public virtual long? transactionStatusId { get; set; }
        public virtual bool transactionStatusIdSpecified { get; set; }
        public virtual string transactionStatus { get; set; }
        public virtual string localizedTransactionStatus { get; set; }
        public virtual long? transactionBaseTypeId { get; set; }
        public virtual bool transactionBaseTypeIdSpecified { get; set; }
        public virtual string transactionBaseType { get; set; }
        public virtual string localizedTransactionBaseType { get; set; }
        public virtual long? categoryId { get; set; }
        public virtual bool categoryIdSpecified { get; set; }
        public virtual long? bankTransactionId { get; set; }
        public virtual bool bankTransactionIdSpecified { get; set; }
        public virtual long? bankAccountId { get; set; }
        public virtual bool bankAccountIdSpecified { get; set; }
        public virtual long? bankStatementId { get; set; }
        public virtual bool bankStatementIdSpecified { get; set; }
        public virtual long? isDeleted { get; set; }
        public virtual bool isDeletedSpecified { get; set; }
        public virtual long? lastUpdated { get; set; }
        public virtual bool lastUpdatedSpecified { get; set; }
        public virtual long? hasDetails { get; set; }
        public virtual bool hasDetailsSpecified { get; set; }
        public virtual string transactionId { get; set; }
        public virtual string transactionCategoryId { get; set; }
        public virtual string siteCategoryType { get; set; }
        public virtual string siteCategory { get; set; }
        public virtual string classUpdationSource { get; set; }
        public virtual string lastCategorised { get; set; }
        public virtual DateTime? transactionDate { get; set; }
        public virtual long? isReimbursable { get; set; }
        public virtual bool isReimbursableSpecified { get; set; }
        public virtual string mcCode { get; set; }
        public virtual long? prevLastCategorised { get; set; }
        public virtual bool prevLastCategorisedSpecified { get; set; }
        public virtual string naicsCode { get; set; }
        public virtual double? runningBalance { get; set; }
        public virtual string userDescription { get; set; }
        public virtual long? customCategoryId { get; set; }
        public virtual bool customCategoryIdSpecified { get; set; }
        public virtual string memo { get; set; }
        public virtual long? parentId { get; set; }
        public virtual bool parentIdSpecified { get; set; }
        public virtual long? isOlbUserDesc { get; set; }
        public virtual bool isOlbUserDescSpecified { get; set; }
        public virtual string categorisationSourceId { get; set; }
        public virtual string plainTextDescription { get; set; }
        public virtual string splitType { get; set; }
        public virtual long? categoryLevelId { get; set; }
        public virtual bool categoryLevelIdSpecified { get; set; }
        public virtual double? calcRunningBalance { get; set; }
        public virtual string category { get; set; }
        public virtual string link { get; set; }
        public virtual DateTime? postDate { get; set; }
        public virtual long? prevTransactionCategoryId { get; set; }
        public virtual bool prevTransactionCategoryIdSpecified { get; set; }
        public virtual long? isBusinessExpense { get; set; }
        public virtual bool isBusinessExpenseSpecified { get; set; }
        public virtual long? descriptionViewPref { get; set; }
        public virtual bool descriptionViewPrefSpecified { get; set; }
        public virtual long? prevCategorisationSourceId { get; set; }
        public virtual bool prevCategorisationSourceIdSpecified { get; set; }
        public virtual double? transactionAmount { get; set; }
        public virtual long? transactionPostingOrder { get; set; }
        public virtual bool transactionPostingOrderSpecified { get; set; }
        public virtual string checkNumber { get; set; }
        public virtual string description { get; set; }
        public virtual long? isTaxDeductible { get; set; }
        public virtual bool isTaxDeductibleSpecified { get; set; }
        public virtual long? isMedicalExpense { get; set; }
        public virtual bool isMedicalExpenseSpecified { get; set; }
        public virtual string categorizationKeyword { get; set; }
        public virtual string sourceTransactionType { get; set; }
    }
}