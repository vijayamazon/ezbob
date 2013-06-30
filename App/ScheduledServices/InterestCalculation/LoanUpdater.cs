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
            var calc = new PayEarlyCalculator2(loan, date);
            calc.GetState();
            loan.InterestDue = calc.InterestToPay;
            loan.LastRecalculation = date;
        }
    }
}