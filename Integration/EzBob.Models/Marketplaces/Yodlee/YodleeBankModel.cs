namespace EzBob.Models.Marketplaces.Yodlee
{
	using System;
	using System.Collections.Generic;

	public class YodleeBankModel
	{
		public YodleeBankModel()
		{
			accountName = "-";
			accountNumber = "-";
			accountType = "-";
			accountHolder = "-";
			currentBalance = null;
			availableBalance = null;
			overdraftProtection = null;
			routingNumber = "-";
			asOfDate = null;
			isDeleted = false;
		}
		public string accountName { get; set; }
		public string accountNumber { get; set; }
		public string accountType { get; set; }
		public string accountHolder { get; set; }
		public double? currentBalance { get; set; }
		public double? availableBalance { get; set; }
		public string routingNumber { get; set; }
		public double? overdraftProtection { get; set; }
		public DateTime? asOfDate { get; set; }
		public bool isDeleted { get; set; }

		public IEnumerable<YodleeTransactionModel> transactions { get; set; }
	}
}