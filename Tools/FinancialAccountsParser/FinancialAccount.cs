namespace FinancialAccountsParser
{
	using System;
	using System.Collections.Generic;

	public class FinancialAccount
	{
		public FinancialAccount()
		{
			StatusCodes = new List<string>();
		}

		public DateTime? StartDate { get; set; }
		public string AccountStatus { get; set; }
		public string DateType { get; set; }
		public DateTime? SettlementOrDefaultDate { get; set; }
		public DateTime? LastUpdateDate { get; set; }
		public List<string> StatusCodes { get; set; }
		public int? CreditLimit { get; set; }
		public int? Balance { get; set; }
		public int? CurrentDefaultBalance { get; set; }
		public int Status1To2 { get; set; }
		public int StatusTo3 { get; set; }
		public string WorstStatus { get; set; }
		public string AccountType { get; set; }
	}
}
