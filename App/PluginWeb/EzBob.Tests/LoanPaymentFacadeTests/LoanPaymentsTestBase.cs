using System;
using System.Globalization;
using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    /// <summary>
    /// A base class for all tests cases that tests loan payment logic
    /// </summary>
    public class LoanPaymentsTestBase
    {
        protected LoanPaymentFacade _facade;
        protected Loan _loan;

        [SetUp]
        public void Init()
        {
            _loan = new Loan();
            _facade = new LoanPaymentFacade();
            SetUp();
        }

        protected virtual void SetUp()
        {
            
        }

        protected void MakePayment(decimal amount, DateTime date)
        {
            Console.WriteLine("Making payment {0} on {1}", amount, date);
            _facade.PayLoan(_loan, "", amount, "", date);
            Console.WriteLine(_loan);
        }

        protected static DateTime Parse(string date)
        {
            return DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss.000", CultureInfo.InvariantCulture);
        }
    }
}