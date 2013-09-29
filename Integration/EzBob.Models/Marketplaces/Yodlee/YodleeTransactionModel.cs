namespace EzBob.Models.Marketplaces
{
	public class YodleeTransactionModel
	{
		public YodleeTransactionModel()
		{
			transactionBaseType = "-";
			transactionDate = "-";
			categoryName = "-";
			categoryType = "-";
			transactionAmount = "-";
			description = "-";
			runningBalance = "-";
		}

		public string transactionBaseType { get; set; }
		public string transactionDate { get; set; }
		public string categoryName { get; set; }
		public string categoryType { get; set; }
		public string transactionAmount { get; set; }
		public string description { get; set; }
		public string runningBalance { get; set; }
	}
}