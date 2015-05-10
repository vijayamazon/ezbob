namespace Ezbob.Backend.CalculateLoan.Models {
	public class CurrentPaymentModel {
		public CurrentPaymentModel() {
			Amount = 0;
			IsLate = false;
		} // constructor

		public CurrentPaymentModel(decimal amount, bool isLate = false) {
			Amount = amount;
			IsLate = isLate;
		} // constructor

		public decimal Amount { get; set; }
		public bool IsLate { get; set; }
	} // class CurrentPaymentModel
} // namespace
