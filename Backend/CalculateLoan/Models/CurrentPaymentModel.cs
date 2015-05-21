namespace Ezbob.Backend.CalculateLoan.Models {
	public class CurrentPaymentModel {
		public CurrentPaymentModel() : this(0) {
		} // constructor

		public CurrentPaymentModel(decimal amount, bool isLate = false, bool loanIsClosed = false, decimal savedAmount = 0) {
			IsError = false;
			Amount = amount;
			IsLate = isLate;
			LoanIsClosed = loanIsClosed;
			SavedAmount = savedAmount;
		} // constructor

		public bool IsError { get; set; }
		public bool LoanIsClosed { get; set; }
		public bool IsLate { get; set; }
		public decimal Amount { get; set; }
		public decimal SavedAmount { get; set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"Loan is closed: {1}, late: {2}, amount: {3}, saving {4} ({0})",
				IsError ? "error" : "no error",
				LoanIsClosed ? "yes" : "no",
				IsLate ? "yes" : "no",
				Amount.ToString("C2", Library.Instance.Culture),
				SavedAmount.ToString("C2", Library.Instance.Culture)
			);
		} // ToString
	} // class CurrentPaymentModel
} // namespace
