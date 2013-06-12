using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using PaymentServices.Calculators;

namespace ScheduledServices.InterestCalculation
{
    public class LoanUpdater
    {
        public void UpdateLoan(Loan loan)
        {
            var calc = new PayEarlyCalculator2(loan, DateTime.UtcNow);
            calc.GetState();
            loan.InterestDue = calc.InterestToPay;
        }
    }
}