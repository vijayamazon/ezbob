using System;
using EZBob.DatabaseLib.Model.Database.Loans;

namespace PaymentServices.Calculators
{
    public interface IPayEarlyCalculatorFactory
    {
        ILoanRepaymentScheduleCalculator Create(Loan loan, DateTime? term);
    }
}