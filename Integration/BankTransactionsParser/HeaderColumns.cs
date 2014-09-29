namespace BankTransactionsParser {
	using System.Data;

	public class HeaderColumns {
		public DataColumn Date { get; set; }
		public DataColumn Amount { get; set; }
		public DataColumn Credit { get; set; }
		public DataColumn Debit { get; set; }
		public DataColumn Description { get; set; }
		public DataColumn Balance { get; set; }

		public HeaderColumns() {
			Date = null;
			Amount = null;
			Credit = null;
			Debit = null;
			Description = null;
			Balance = null;
		}

		public bool HaveMinimumColumns() {
			return (Amount != null || (Credit != null && Debit != null)) && 
				Date != null && 
				Description != null;
		}

		public override string ToString() {
			return string.Format("Date: {0} Amount: {1} Credit:{2} Debit:{3} Description:{4} Balance:{5}", 
				Date.ColumnName ?? "not found",
				Amount.ColumnName ?? "not found",
				Credit.ColumnName ?? "not found",
				Debit.ColumnName ?? "not found",
				Description.ColumnName ?? "not found",
				Balance.ColumnName ?? "not found");
		}
	}
}
