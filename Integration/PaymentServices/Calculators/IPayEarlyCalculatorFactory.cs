using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using EzBob.Web.Areas.Customer.Models;

namespace PaymentServices.Calculators
{
    public interface IPayEarlyCalculatorFactory
    {
        ILoanRepaymentScheduleCalculator Create(Loan loan, DateTime? term);
    }
}