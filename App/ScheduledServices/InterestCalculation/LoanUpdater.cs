using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using PaymentServices.Calculators;

namespace ScheduledServices.InterestCalculation
{
    public class LoanUpdater
    {
        public void UpdateLoan(Loan loan)
        {
            var date = DateTime.UtcNow;
            var calc = new LoanRepaymentScheduleCalculator(loan, date);
            var state = calc.GetState();
            loan.InterestDue = state.Interest;
            loan.LastRecalculation = date;
        }
    }
}