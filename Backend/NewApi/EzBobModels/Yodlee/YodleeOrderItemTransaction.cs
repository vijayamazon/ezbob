using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Yodlee
{
    using System.Security.Policy;

    /// <summary>
    /// Yodlee v9 model. Represents transaction.
    /// </summary>
    public class YodleeOrderItemTransaction
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public int? isSeidFromDataSource { get; set; }
        public bool? isSeidFromDataSourceSpecified { get; set; }
        public int? isSeidMod { get; set; }
        public bool? isSeidModSpecified { get; set; }
        public string srcElementId { get; set; }
        public int? transactionTypeId { get; set; }
        public bool? transactionTypeIdSpecified { get; set; }
        public string transactionType { get; set; }
        public string localizedTransactionType { get; set; }
        public int? transactionStatusId { get; set; }
        public bool? transactionStatusIdSpecified { get; set; }
        public string transactionStatus { get; set; }
        public string localizedTransactionStatus { get; set; }
        public int? transactionBaseTypeId { get; set; }
        public bool? transactionBaseTypeIdSpecified { get; set; }
        public string transactionBaseType { get; set; }
        public string localizedTransactionBaseType { get; set; }
        public int? categoryId { get; set; }
        public bool? categoryIdSpecified { get; set; }
        public int? bankTransactionId { get; set; }
        public bool? bankTransactionIdSpecified { get; set; }
        public int bankAccountId { get; set; }
        public bool? bankAccountIdSpecified { get; set; }
        public int? bankStatementId { get; set; }
        public bool? bankStatementIdSpecified { get; set; }
        public int? isDeleted { get; set; }
        public bool? isDeletedSpecified { get; set; }
        public int? lastUpdated { get; set; }
        public bool? lastUpdatedSpecified { get; set; }
        public int? hasDetails { get; set; }
        public bool? hasDetailsSpecified { get; set; }
        public string transactionId { get; set; }
        public string transactionCategoryId { get; set; }
        public string siteCategoryType { get; set; }
        public string siteCategory { get; set; }
        public string classUpdationSource { get; set; }
        public string lastCategorised { get; set; }
        public DateTime transactionDate { get; set; }
        public int? isReimbursable { get; set; }
        public bool? isReimbursableSpecified { get; set; }
        public string mcCode { get; set; }
        public int? prevLastCategorised { get; set; }
        public bool? prevLastCategorisedSpecified { get; set; }
        public string naicsCode { get; set; }
        public decimal? runningBalance { get; set; }
        public string runningBalanceCurrency { get; set; }
        public string userDescription { get; set; }
        public int? customCategoryId { get; set; }
        public bool? customCategoryIdSpecified { get; set; }
        public string memof { get; set; }
        public int? parentId { get; set; }
        public bool? parentIdSpecified { get; set; }
        public int? isOlbUserDesc { get; set; }
        public bool? isOlbUserDescSpecified { get; set; }
        public string categorisationSourceId { get; set; }
        public string plainTextDescription { get; set; }
        public string splitType { get; set; }
        public int? categoryLevelId { get; set; }
        public bool? categoryLevelIdSpecified { get; set; }
        public decimal? calcRunningBalance { get; set; }
        public string calcRunningBalanceCurrency { get; set; }
        public string category { get; set; }
        public string link { get; set; }
        public DateTime? postDate { get; set; }
        public int? prevTransactionCategoryId { get; set; }
        public bool? prevTransactionCategoryIdSpecified { get; set; }
        public int? isBusinessExpense { get; set; }
        public bool? isBusinessExpenseSpecified { get; set; }
        public int? descriptionViewPref { get; set; }
        public bool? descriptionViewPrefSpecified { get; set; }
        public int? prevCategorisationSourceId { get; set; }
        public bool? prevCategorisationSourceIdSpecified { get; set; }
        public decimal? transactionAmount { get; set; }
        public string transactionAmountCurrency { get; set; }
        public int? transactionPostingOrder { get; set; }
        public bool? transactionPostingOrderSpecified { get; set; }
        public string checkNumber { get; set; }
        public string description { get; set; }
        public int? isTaxDeductible { get; set; }
        public bool? isTaxDeductibleSpecified { get; set; }
        public int? isMedicalExpense { get; set; }
        public bool? isMedicalExpenseSpecified { get; set; }
        public string categorizationKeyword { get; set; }
        public string sourceTransactionType { get; set; }
        public string EzbobCategory { get; set; }
    }
}
