namespace EzBob.Models.Marketplaces.Yodlee
{
	using System.Collections.Generic;

	public class YodleeModel
	{
		public IEnumerable<YodleeBankModel> banks { get; set; }
		public YodleeCashFlowReportModel CashFlowReportModel { get; set; }
		public YodleeSearchWordsModel SearchWordsModel { get; set; }
	}


}