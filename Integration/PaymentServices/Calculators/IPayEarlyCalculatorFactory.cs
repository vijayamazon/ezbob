using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using EzBob.Web.Areas.Customer.Models;

namespace PaymentServices.Calculators
{
    public interface IPayEarlyCalculatorFactory
    {
        IPayEarlyCalculator Create(Loan loan, DateTime? term);
    }
}