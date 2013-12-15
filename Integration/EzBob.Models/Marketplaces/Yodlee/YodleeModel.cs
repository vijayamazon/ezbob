namespace EzBob.Models.Marketplaces.Yodlee
{
	using System;
	using System.Collections.Generic;

	[Serializable]
	public class YodleeModel
	{
		public IEnumerable<YodleeBankModel> banks { get; set; }
		public YodleeCashFlowReportModel CashFlowReportModel { get; set; }
		public YodleeSearchWordsModel SearchWordsModel { get; set; }
		public YodleeRunningBalanceModel RunningBalanceModel { get; set; }
		public YodleeRuleModel RuleModel { get; set; }
		public BankStatementDataModel BankStatementDataModel { get; set; }
	}
}