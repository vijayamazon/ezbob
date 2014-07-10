namespace EZBob.DatabaseLib.Model.Marketplaces.Yodlee
{
	using System;
	using Iesi.Collections.Generic;
	/// <summary>
	/// commented out all the fields that are always null from yodlee to improve performance
	/// </summary>
	public class MP_YodleeOrderItem
	{
		public MP_YodleeOrderItem()
		{
			OrderItemBankTransactions = new HashedSet<MP_YodleeOrderItemBankTransaction>();
		}

		public virtual int Id { get; set; }

		public virtual MP_YodleeOrder Order { get; set; }

		public virtual ISet<MP_YodleeOrderItemBankTransaction> OrderItemBankTransactions { get; set; }

		public virtual long? isSeidFromDataSource { get; set; }
		//public virtual bool isSeidFromDataSourceSpecified { get; set; }
		public virtual long? isSeidMod { get; set; }
		//public virtual bool isSeidModSpecified { get; set; }
		public virtual long? acctTypeId { get; set; }
		//public virtual bool acctTypeIdSpecified { get; set; }
		public virtual string acctType { get; set; }
		public virtual string localizedAcctType { get; set; }
		public virtual string srcElementId { get; set; }
		//public virtual long? individualInformationId { get; set; }
		//public virtual bool individualInformationIdSpecified { get; set; }
		public virtual long? bankAccountId { get; set; }
		//public virtual bool bankAccountIdSpecified { get; set; }
		//public virtual string customName { get; set; }
		//public virtual string customDescription { get; set; }
		public virtual long? isDeleted { get; set; }
		//public virtual bool isDeletedSpecified { get; set; }
		public virtual long? lastUpdated { get; set; }
		//public virtual bool lastUpdatedSpecified { get; set; }
		public virtual long? hasDetails { get; set; }
		//public virtual bool hasDetailsSpecified { get; set; }
		public virtual double? interestRate { get; set; }
		//public virtual bool interestRateSpecified { get; set; }
		public virtual string accountNumber { get; set; }
		public virtual string link { get; set; }
		public virtual string accountHolder { get; set; }
		public virtual DateTime? tranListToDate { get; set; }
		public virtual DateTime? tranListFromDate { get; set; }
		public virtual double? availableBalance { get; set; }
		public virtual string availableBalanceCurrency { get; set; }
		public virtual double? currentBalance { get; set; }
		public virtual string currentBalanceCurrency { get; set; }
		//public virtual double? interestEarnedYtd { get; set; }
		//public virtual string interestEarnedYtdCurrency { get; set; }
		//public virtual double? prevYrInterest { get; set; }
		//public virtual string prevYrInterestCurrency { get; set; }
		public virtual double? overdraftProtection { get; set; }
		public virtual string overdraftProtectionCurrency { get; set; }
		//public virtual string term { get; set; }
		public virtual string accountName { get; set; }
		//public virtual double? annualPercentYield { get; set; }
		//public virtual bool annualPercentYieldSpecified { get; set; }
		public virtual string routingNumber { get; set; }
		public virtual DateTime? maturityDate { get; set; }
		public virtual DateTime? asOfDate { get; set; }
		//public virtual object[] bankStatements { get; set; }
		//public virtual IndividualInformation individualInformation { get; set; }
		//public virtual string accountNicknameAtSrcSite { get; set; }
		public virtual long? isPaperlessStmtOn { get; set; }
		//public virtual bool isPaperlessStmtOnSpecified { get; set; }
		public virtual string siteAccountStatus { get; set; }
		//public virtual bool siteAccountStatusSpecified { get; set; }
		public virtual long? created { get; set; }
		//public virtual bool createdSpecified { get; set; }
		//public virtual string nomineeName { get; set; }
		public virtual string secondaryAccountHolderName { get; set; }
		public virtual DateTime? accountOpenDate { get; set; }
		public virtual DateTime? accountCloseDate { get; set; }
		//public virtual double? maturityAmount { get; set; }
		//public virtual string maturityAmountCurrency { get; set; }
		//public virtual double? taxesWithheldYtd { get; set; }
		//public virtual string taxesWithheldYtdCurrency { get; set; }
		//public virtual double? taxesPaidYtd { get; set; }
		//public virtual string taxesPaidYtdCurrency { get; set; }
		//public virtual double? budgetBalance { get; set; }
		//public virtual string budgetBalanceCurrency { get; set; }
		//public virtual double? straightBalance { get; set; }
		//public virtual string straightBalanceCurrency { get; set; }
		public virtual string accountClassification { get; set; }
		//public virtual bool accountClassificationSpecified { get; set; }
		public virtual long? itemAccountId { get; set; }
	}
}