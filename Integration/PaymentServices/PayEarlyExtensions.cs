using System;
using System.Linq;
using EZBob.DatabaseLib.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using StructureMap;

namespace PaymentServices.Calculators
{
    public static class PayEarlyExtensions
    {
        public static decimal NextEarlyPayment(this Loan loan, DateTime? term = null)
        {
            var configVariables = ObjectFactory.TryGetInstance<IConfigurationVariablesRepository>();
            var calc = new LoanRepaymentScheduleCalculator(loan, term, configVariables);
            return calc.NextEarlyPayment();
        }

        public static decimal TotalEarlyPayment(this Loan loan, DateTime? term = null)
        {
            var configVariables = ObjectFactory.TryGetInstance<IConfigurationVariablesRepository>();
            var calc = new LoanRepaymentScheduleCalculator(loan, term, configVariables);
            return calc.TotalEarlyPayment();
        }

        public static decimal TotalEarlyPayment(this Customer customer, DateTime? term = null)
        {
            return customer.Loans.Sum(l => l.TotalEarlyPayment(term));
        }
    }
}