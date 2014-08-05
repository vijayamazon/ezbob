namespace EzBob.Models.Marketplaces.Yodlee
{
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Models;

	[Serializable]
	public class YodleeModel
	{
		public List<YodleeBankModel> banks { get; set; }
		public YodleeCashFlowReportModel CashFlowReportModel { get; set; }
		public YodleeSearchWordsModel SearchWordsModel { get; set; }
		public YodleeRunningBalanceModel RunningBalanceModel { get; set; }
		public YodleeRuleModel RuleModel { get; set; }
		public BankStatementDataModel BankStatementDataModel { get; set; }
		public BankStatementDataModel BankStatementAnnualizedModel { get; set; }
	}
}