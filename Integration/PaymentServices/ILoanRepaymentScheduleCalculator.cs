namespace PaymentServices
{
	using EZBob.DatabaseLib.Model.Database.Loans;

	public interface ILoanRepaymentScheduleCalculator
    {
        decimal NextEarlyPayment();
        decimal TotalEarlyPayment();
        decimal RecalculateSchedule();
        LoanScheduleItem GetState();
    }
}