using System;

namespace Reconciliation {
	public class PacNetBalanceRow {
		public DateTime Date { get; set; }
		public decimal Amount { get; set; }
		public decimal Fees { get; set; }
		public decimal CurrentBalance { get; set; }
		public bool IsCredit { get; set; }
	} // class PacNetBalanceRow
} // namespace
