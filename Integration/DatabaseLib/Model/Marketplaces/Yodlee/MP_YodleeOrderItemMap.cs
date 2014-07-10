using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	/// <summary>
	/// commented out all the fields that are always null from yodlee to improve performance
	/// </summary>
	public class MP_YodleeOrderItemMap : ClassMap<MP_YodleeOrderItem>
	{
		public MP_YodleeOrderItemMap()
		{
			Table("MP_YodleeOrderItem");
			Id(x => x.Id);
			References(x => x.Order).Column("OrderId");
			HasMany(x => x.OrderItemBankTransactions).KeyColumn("OrderItemId").Cascade.All();

			Map(x => x.isSeidFromDataSource, "isSeidFromDataSource").Nullable();
			//Map(x => x.isSeidFromDataSourceSpecified, "isSeidFromDataSourceSpecified");
			Map(x => x.isSeidMod, "isSeidMod").Nullable();
			//Map(x => x.isSeidModSpecified, "isSeidModSpecified").Length(300);
			Map(x => x.acctTypeId, "acctTypeId").Length(300);
			//Map(x => x.acctTypeIdSpecified, "acctTypeIdSpecified").Length(300);
			Map(x => x.acctType, "acctType").Length(300);
			Map(x => x.localizedAcctType, "localizedAcctType").Length(300);
			Map(x => x.srcElementId, "srcElementId").Length(300);
			//Map(x => x.individualInformationId, "individualInformationId").Nullable();
			//Map(x => x.individualInformationIdSpecified, "individualInformationIdSpecified");
			Map(x => x.bankAccountId, "bankAccountId").Nullable();
			//Map(x => x.bankAccountIdSpecified, "bankAccountIdSpecified");
			//Map(x => x.customName, "customName").Length(300);
			//Map(x => x.customDescription, "customDescription").Length(300);
			Map(x => x.isDeleted, "isDeleted").Nullable();
			//Map(x => x.isDeletedSpecified, "isDeletedSpecified");
			Map(x => x.lastUpdated, "lastUpdated").Nullable();
			//Map(x => x.lastUpdatedSpecified, "lastUpdatedSpecified");
			Map(x => x.hasDetails, "hasDetails").Nullable();
			//Map(x => x.hasDetailsSpecified, "hasDetailsSpecified");
			Map(x => x.interestRate, "interestRate").Nullable();
			//Map(x => x.interestRateSpecified, "interestRateSpecified");
			Map(x => x.accountNumber, "accountNumber").Length(300);
			Map(x => x.link, "link").Length(300);
			Map(x => x.accountHolder, "accountHolder").Length(300);
			Map(x => x.tranListToDate, "tranListToDate").CustomType<UtcDateTimeType>().Nullable();
			Map(x => x.tranListFromDate, "tranListFromDate").CustomType<UtcDateTimeType>().Nullable();
			Map(x => x.availableBalance, "availableBalance").Nullable();
			Map(x => x.availableBalanceCurrency, "availableBalanceCurrency").Length(3).Nullable();
			Map(x => x.currentBalance, "currentBalance").Nullable();
			Map(x => x.currentBalanceCurrency, "currentBalanceCurrency").Length(3).Nullable();
			//Map(x => x.interestEarnedYtd, "interestEarnedYtd").Nullable();
			//Map(x => x.interestEarnedYtdCurrency, "interestEarnedYtdCurrency").Length(3).Nullable();
			//Map(x => x.prevYrInterest, "prevYrInterest").Nullable();
			//Map(x => x.prevYrInterestCurrency, "prevYrInterestCurrency").Length(3).Nullable();
			Map(x => x.overdraftProtection, "overdraftProtection").Nullable();
			Map(x => x.overdraftProtectionCurrency, "overdraftProtectionCurrency").Length(3).Nullable();
			//Map(x => x.term, "term").Length(300);
			Map(x => x.accountName, "accountName").Length(300);
			//Map(x => x.annualPercentYield, "annualPercentYield").Nullable();
			//Map(x => x.annualPercentYieldSpecified, "annualPercentYieldSpecified");
			Map(x => x.routingNumber, "routingNumber").Length(300);
			Map(x => x.maturityDate, "maturityDate").CustomType<UtcDateTimeType>().Nullable();
			Map(x => x.asOfDate, "asOfDate").CustomType<UtcDateTimeType>().Nullable();
			//Map(x => x.accountNicknameAtSrcSite, "accountNicknameAtSrcSite").Length(300);
			Map(x => x.isPaperlessStmtOn, "isPaperlessStmtOn").Nullable();
			//Map(x => x.isPaperlessStmtOnSpecified, "isPaperlessStmtOnSpecified");
			Map(x => x.siteAccountStatus, "siteAccountStatus").Nullable().Length(50);
			//Map(x => x.siteAccountStatusSpecified, "siteAccountStatusSpecified");
			Map(x => x.created, "created").Nullable();
			//Map(x => x.createdSpecified, "createdSpecified");
			//Map(x => x.nomineeName, "nomineeName").Length(300);
			Map(x => x.secondaryAccountHolderName, "secondaryAccountHolderName").Length(300);
			Map(x => x.accountOpenDate, "accountOpenDate").CustomType<UtcDateTimeType>().Nullable();
			Map(x => x.accountCloseDate, "accountCloseDate").CustomType<UtcDateTimeType>().Nullable();
			//Map(x => x.maturityAmount, "maturityAmount").Nullable();
			//Map(x => x.maturityAmountCurrency, "maturityAmountCurrency").Length(3).Nullable();
			//Map(x => x.taxesWithheldYtd, "taxesWithheldYtd").Nullable();
			//Map(x => x.taxesWithheldYtdCurrency, "taxesWithheldYtdCurrency").Length(3).Nullable();
			//Map(x => x.taxesPaidYtd, "taxesPaidYtd").Nullable();
			//Map(x => x.taxesPaidYtdCurrency, "taxesPaidYtdCurrency").Length(3).Nullable();
			//Map(x => x.budgetBalance, "budgetBalance").Nullable();
			//Map(x => x.budgetBalanceCurrency, "budgetBalanceCurrency").Length(3).Nullable();
			//Map(x => x.straightBalance, "straightBalance").Nullable();
			//Map(x => x.straightBalanceCurrency, "straightBalanceCurrency").Length(3).Nullable();
			Map(x => x.accountClassification, "accountClassification").Nullable().Length(50);
			//Map(x => x.accountClassificationSpecified, "accountClassificationSpecified");
			Map(x => x.itemAccountId, "itemAccountId");
		}
	}
}