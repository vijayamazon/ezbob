namespace BankTransactionsParser {
	using System;
	using System.Globalization;

	public class BankTransaction {
		public DateTime Date { get; set; }
		public decimal Amount { get; set; }
		public bool IsCredit { get; set; }
		public string Description { get; set; }
		public decimal? Balance { get; set; }

		public override string ToString() {
			return string.Format("Date: {0} Amount: {1} {5} Description: {2} {3} {4}",
				Date, Amount, Description, Balance.HasValue ? "Balance: " : "", Balance.HasValue ? Balance.Value.ToString(CultureInfo.InvariantCulture) : "", IsCredit? "credit" : "debit");
		}
	}
}
