namespace BankTransactionsParser {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;

	public class ParsedBankAccount {
		public string Name { get; set; }
		public DateTime DateFrom { get; set; }
		public DateTime DateTo { get; set; }
		public decimal? Balance { get; set; }
		public int NumOfTransactions { get; set; }
		public List<ParsedBankTransaction> Transactions { get; set; }
        public string Error { get; set; }
		public override string ToString() {

			var numCredit = Transactions!= null ? Transactions.Count(t => t.IsCredit) : 0;
			var numDebit = Transactions!= null ? Transactions.Count(t => !t.IsCredit) : 0;
			var firstTran = Transactions != null && Transactions.Any() ? Transactions[0].ToString() : "";
			return string.Format("Name: {0} From: {1} To: {2} {3} {4} # Transactions: {5} #credit: {7} #debit: {8}\n First Transaction: {6}",
				Name, DateFrom, DateTo, Balance.HasValue ? "Balance: " : "", Balance.HasValue ? Balance.Value.ToString(CultureInfo.InvariantCulture) : "",
				NumOfTransactions, firstTran, numCredit, numDebit);
		}
	}
}
