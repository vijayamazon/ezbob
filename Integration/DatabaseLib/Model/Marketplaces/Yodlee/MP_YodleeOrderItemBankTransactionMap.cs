using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	/// <summary>
	/// commented out all the fields that are always null from yodlee to improve performance
	/// </summary>
    public class MP_YodleeOrderItemBankTransactionMap : ClassMap<MP_YodleeOrderItemBankTransaction>
    {
        public MP_YodleeOrderItemBankTransactionMap()
        {
            Table("MP_YodleeOrderItemBankTransaction");
            Id(x => x.Id);
            References(x => x.YodleeOrderItem).Column("OrderItemId");

            Map(x => x.isSeidFromDataSource).Nullable();
            //Map(x => x.isSeidFromDataSourceSpecified);
            Map(x => x.isSeidMod).Nullable();
            //Map(x => x.isSeidModSpecified);
            Map(x => x.srcElementId).Length(300);
            Map(x => x.transactionTypeId).Nullable();
            //Map(x => x.transactionTypeIdSpecified);
            Map(x => x.transactionType).Length(300);
            //Map(x => x.localizedTransactionType).Length(300);
            Map(x => x.transactionStatusId).Nullable();
            //Map(x => x.transactionStatusIdSpecified);
            Map(x => x.transactionStatus).Length(300);
            //Map(x => x.localizedTransactionStatus).Length(300);
            Map(x => x.transactionBaseTypeId).Nullable();
            //Map(x => x.transactionBaseTypeIdSpecified);
            Map(x => x.transactionBaseType).Length(300);
            //Map(x => x.localizedTransactionBaseType).Length(300);
            Map(x => x.categoryId).Nullable();
            //Map(x => x.categoryIdSpecified);
            Map(x => x.bankTransactionId).Nullable();
            //Map(x => x.bankTransactionIdSpecified);
            Map(x => x.bankAccountId).Nullable();
            //Map(x => x.bankAccountIdSpecified);
            Map(x => x.bankStatementId).Nullable();
            //Map(x => x.bankStatementIdSpecified);
            Map(x => x.isDeleted).Nullable();
            //Map(x => x.isDeletedSpecified);
            Map(x => x.lastUpdated).Nullable();
            //Map(x => x.lastUpdatedSpecified);
            Map(x => x.hasDetails).Nullable();
            //Map(x => x.hasDetailsSpecified);
            Map(x => x.transactionId).Length(300);
            References(x => x.transactionCategory).Column("transactionCategoryId").Fetch.Join();
            //Map(x => x.siteCategoryType).Length(300);
            //Map(x => x.siteCategory).Length(300);
            Map(x => x.classUpdationSource).Length(300);
            Map(x => x.lastCategorised).Length(300);
            Map(x => x.transactionDate).CustomType<UtcDateTimeType>().Nullable();
            //Map(x => x.isReimbursable).Nullable();
            //Map(x => x.isReimbursableSpecified);
            //Map(x => x.mcCode).Length(300);
            Map(x => x.prevLastCategorised).Nullable();
            //Map(x => x.prevLastCategorisedSpecified).Nullable();
            //Map(x => x.naicsCode).Length(300);
            Map(x => x.runningBalance).Nullable();
            Map(x => x.runningBalanceCurrency).Length(3);
            //Map(x => x.userDescription).Length(300);
            //Map(x => x.customCategoryId).Nullable();
            //Map(x => x.customCategoryIdSpecified);
            //Map(x => x.memo).Length(300);
            //Map(x => x.parentId).Nullable();
            //Map(x => x.parentIdSpecified);
            //Map(x => x.isOlbUserDesc).Nullable();
            //Map(x => x.isOlbUserDescSpecified);
            Map(x => x.categorisationSourceId).Length(300);
            Map(x => x.plainTextDescription).Length(300);
            //Map(x => x.splitType).Length(300);
            //Map(x => x.categoryLevelId).Nullable();
            //Map(x => x.categoryLevelIdSpecified);
            Map(x => x.calcRunningBalance).Nullable();
            Map(x => x.calcRunningBalanceCurrency).Length(3);
            Map(x => x.category).Length(300);
            Map(x => x.link).Length(300);
            Map(x => x.postDate).CustomType<UtcDateTimeType>().Nullable();
            Map(x => x.prevTransactionCategoryId).Nullable();
            //Map(x => x.prevTransactionCategoryIdSpecified);
            //Map(x => x.isBusinessExpense).Nullable();
            //Map(x => x.isBusinessExpenseSpecified);
            Map(x => x.descriptionViewPref).Nullable();
            //Map(x => x.descriptionViewPrefSpecified);
            Map(x => x.prevCategorisationSourceId).Nullable();
            //Map(x => x.prevCategorisationSourceIdSpecified);
            Map(x => x.transactionAmount).Nullable();
            Map(x => x.transactionAmountCurrency).Length(3);
            //Map(x => x.transactionPostingOrder).Nullable();
            //Map(x => x.transactionPostingOrderSpecified);
            Map(x => x.checkNumber).Length(300);
            Map(x => x.description).Length(300);
           // Map(x => x.isTaxDeductible).Nullable();
           // Map(x => x.isTaxDeductibleSpecified);
           // Map(x => x.isMedicalExpense).Nullable();
           // Map(x => x.isMedicalExpenseSpecified);
            Map(x => x.categorizationKeyword).Length(300);
           // Map(x => x.sourceTransactionType).Length(300);
	        References(x => x.ezbobCategory, "EzbobCategory");
        }
    }
}