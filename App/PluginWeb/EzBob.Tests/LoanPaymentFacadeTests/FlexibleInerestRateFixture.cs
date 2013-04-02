using System;
using System.Globalization;
using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    [TestFixture]
    public class FlexibleInerestRateFixture
    {
        private Loan _loan;
        private LoanPaymentFacade _facade;

        [SetUp]
        public void SetUp()
        {
            _loan = new Loan();
            _facade = new LoanPaymentFacade();
        }

        [Test]
        public void can_recalculate_simple_flexible_ir_loan()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.2M, Term = 2};
            calculator.Calculate(1000, _loan, Parse("2012-01-01 12:00:00.000"));

            _loan.Schedule[1].InterestRate = 0.1M;

            _facade.GetStateAt(_loan, _loan.Date);

            Assert.That(_loan.Schedule[0].Interest, Is.EqualTo(200));
            Assert.That(_loan.Schedule[1].Interest, Is.EqualTo(50));
        }

         private void MakePayment(decimal amount, DateTime date)
         {
             Console.WriteLine("Making payment {0} on {1}", amount, date);
             _facade.PayLoan(_loan, "", amount, "", date);
             Console.WriteLine(_loan);
         }

         private static DateTime Parse(string date)
         {
             return DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss.000", CultureInfo.InvariantCulture);
         }

    }
}