using System;
namespace EzBob.Models.Marketplaces
{
	public class YodleeTransactionModel
	{
		public YodleeTransactionModel()
		{
			transactionBaseType = "-";
			transactionDate = null;
			categoryName = "-";
			categoryType = "-";
			transactionAmount = null;
			description = "-";
			runningBalance = null;
			bankTransactionId = null;
		}

		public string transactionBaseType { get; set; }
		public DateTime? transactionDate { get; set; }
		public string categoryName { get; set; }
		public string categoryType { get; set; }
		public double? transactionAmount { get; set; }
		public string description { get; set; }
		public double? runningBalance { get; set; }
		public string transactionStatus { get; set; }
		public long? bankTransactionId { get; set; }
	}
}