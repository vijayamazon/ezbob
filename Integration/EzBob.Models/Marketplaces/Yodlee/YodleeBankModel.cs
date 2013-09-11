namespace EzBob.Models.Marketplaces.Yodlee
{
	using System.Collections.Generic;

	public class YodleeBankModel
	{
		public YodleeBankModel()
		{
			customName = "-";
			customDescription = "-";
			isDeleted = "-";
			accountNumber = "-";
			accountHolder = "-";
			availableBalance = "-";
			term = "-";
			accountName = "-";
			routingNumber = "-";
			accountNicknameAtSrcSite = "-";
			secondaryAccountHolderName = "-";
			accountOpenDate = "-";
			taxesWithheldYtd = "-";
		}
		public string customName { get; set; }
		public string customDescription { get; set; }
		public string isDeleted { get; set; }
		public string accountNumber { get; set; }
		public string accountHolder { get; set; }
		public string availableBalance { get; set; }
		public string term { get; set; }
		public string accountName { get; set; }
		public string routingNumber { get; set; }
		public string accountNicknameAtSrcSite { get; set; }
		public string secondaryAccountHolderName { get; set; }
		public string accountOpenDate { get; set; }
		public string taxesWithheldYtd { get; set; }
		public IEnumerable<YodleeTransactionModel> transactions { get; set; }
	}
}