namespace Ezbob.Backend.CalculateLoan.LoanCalculator.Exceptions {
	public class InvalidPaymentAmountToAssignException : ALoanCalculatorException {
		public InvalidPaymentAmountToAssignException(decimal amount)
			: base(string.Format("Invalid payment amount {0} to assign to loan was specified.", amount.ToString("C2", Library.Instance.Culture)))
		{
		} 
	} 
} 