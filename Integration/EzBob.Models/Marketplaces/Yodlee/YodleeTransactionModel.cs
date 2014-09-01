using System;
namespace EzBob.Models.Marketplaces
{
	[Serializable]
	public class YodleeTransactionModel
	{
		public YodleeTransactionModel()
		{
			transactionBaseType = "-";
			transactionType = "-";
			transactionDate = new DateTime();
			ezbobGroup = "-";
			ezbobSubGroup = "-";
			categoryName = "-";
			categoryType = "-";
			transactionAmount = null;
			description = "-";
			runningBalance = null;
			bankTransactionId = null;
		}

		public string transactionBaseType { get; set; }
		public string transactionType { get; set; }
		public DateTime transactionDate { get; set; }
		public string ezbobGroup { get; set; }
		public string ezbobSubGroup { get; set; }
		public string categoryName { get; set; }
		public string categoryType { get; set; }
		public double? transactionAmount { get; set; }
		public string description { get; set; }
		public double? runningBalance { get; set; }
		public string transactionStatus { get; set; }
		public long? bankTransactionId { get; set; }
		public long ezbobGroupPriority { get; set; }
		
	}
}