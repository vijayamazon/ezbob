namespace Ezbob.Backend.CalculateLoan.Models {
	public class CurrentPaymentModel {
		public CurrentPaymentModel() : this(0) {
		} // constructor

		public CurrentPaymentModel(decimal amount, bool isLate = false, bool loanIsClosed = false, decimal savedAmount = 0) {
			Amount = amount;
			IsLate = isLate;
			LoanIsClosed = loanIsClosed;
			SavedAmount = savedAmount;
		} // constructor

		public bool IsError { get; set; }
		public decimal Amount { get; set; }
		public bool IsLate { get; set; }
		public bool LoanIsClosed { get; set; }
		public decimal SavedAmount { get; set; }
	} // class CurrentPaymentModel
} // namespace
