namespace Ezbob.Backend.CalculateLoan.Models {
	public class CurrentPaymentModel {
		public CurrentPaymentModel(
			decimal amount = 0,
			bool isLate = false,
			bool loanIsClosed = false,
			decimal savedAmount = 0,
			decimal balance = 0,
			decimal accruedInterest = 0
		) {
			IsError = false;
			Amount = amount;
			IsLate = isLate;
			LoanIsClosed = loanIsClosed;
			SavedAmount = savedAmount;
			Balance = balance;
			AccruedInterest = accruedInterest;
		} // constructor

		/// <summary>
		/// There was/was not an error during calculation. Possible errors:
		/// 1. requested date is before loan issue date.
		/// 2. there was a failure creating daily loan status list.
		/// </summary>
		public bool IsError { get; set; }

		/// <summary>
		/// Indicates whether the loan is closed on requested date.
		/// </summary>
		public bool LoanIsClosed { get; set; }

		/// <summary>
		/// Indicates whether customer is late for this loan on requested date.
		/// </summary>
		public bool IsLate { get; set; }

		/// <summary>
		/// Amount to charge on requested date.
		/// </summary>
		public decimal Amount { get; set; }

		/// <summary>
		/// If customer pays next installment now instead of paying it on its date
		/// this amount would be saved.
		/// </summary>
		public decimal SavedAmount { get; set; }

		/// <summary>
		/// Paying this amount now will close the loan immediately.
		/// </summary>
		public decimal Balance { get; set; }

		/// <summary>
		/// Interest that customer should pay today (e.g. for rollover).
		/// </summary>
		public decimal AccruedInterest { get; set; }

		public string ScenarioName { get; set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"scenario '{6}', loan is closed: {1}, late: {2}, amount: {3}, " +
				"saving {4}, balance: {5}, accrued interest: {7} ({0})",
				IsError ? "error" : "no error",
				LoanIsClosed ? "yes" : "no",
				IsLate ? "yes" : "no",
				Amount.ToString("C2", Library.Instance.Culture),
				SavedAmount.ToString("C2", Library.Instance.Culture),
				Balance.ToString("C2", Library.Instance.Culture),
				ScenarioName,
				AccruedInterest.ToString("C2", Library.Instance.Culture)
			);
		} // ToString
	} // class CurrentPaymentModel
} // namespace
