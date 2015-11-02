namespace Ezbob.Backend.CalculateLoan.LoanCalculator {
	interface ILoanCalculator {

		decimal NextEarlyPayment();
		decimal TotalEarlyPayment();
		decimal RecalculateSchedule();
		void GetState();
	}
}
