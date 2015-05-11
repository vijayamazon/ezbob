namespace Ezbob.Backend.CalculateLoan.Models {
	public class CurrentPaymentModel {
		public CurrentPaymentModel() {
			Amount = 0;
			IsLate = false;
			LoanIsClosed = false;
		} // constructor

		public CurrentPaymentModel(decimal amount, bool isLate = false, bool loanIsClosed = false) {
			Amount = amount;
			IsLate = isLate;
			LoanIsClosed = loanIsClosed;
		} // constructor

		public bool IsError { get; set; }
		public decimal Amount { get; set; }
		public bool IsLate { get; set; }
		public bool LoanIsClosed { get; set; }
	} // class CurrentPaymentModel
} // namespace
