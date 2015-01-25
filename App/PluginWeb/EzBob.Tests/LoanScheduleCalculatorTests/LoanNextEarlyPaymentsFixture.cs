using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests
{
    [TestFixture]
    public class LoanNextEarlyPaymentsFixture
    {
        private LoanScheduleCalculator _calculator;

        [SetUp]
        public void SetUp()
        {
            _calculator = new LoanScheduleCalculator();
        }

        [Test]
        public void loan_of_3k()
        {
            var loan = new Loan() { };
            _calculator.Calculate(3000, loan, new DateTime(2012, 1, 1));

            var nextEarlyFor15Jan = NextPayment(loan, new DateTime(2012, 1, 15));

            Assert.That(nextEarlyFor15Jan, Is.EqualTo(1081.29));
        }

        [Test]
        public void loan_of_3k_paytoday()
        {
            var loan = new Loan() { };
            _calculator.Calculate(3000, loan, new DateTime(2012, 1, 1));

            var nextEarlyFor15Jan = NextPayment(loan, new DateTime(2012, 1, 1));

            Assert.That(nextEarlyFor15Jan, Is.EqualTo(1000));
        }

        [Test]
        public void loan_after_payment_today()
        {
            var loan = new Loan();
            var startDate = new DateTime(2012, 1, 1);

            _calculator.Calculate(100, loan, startDate);

            Console.WriteLine(loan);

            MakeEarlyPayment(loan, 92m, startDate);

            Console.WriteLine(loan);

            var nextPayment = NextPayment(loan, startDate);

            Assert.That(nextPayment, Is.EqualTo(8));
        }

        [Test]
        public void payment_on_the_day_of_installment()
        {
            var loan = new Loan();
            var startDate = new DateTime(2012, 1, 1);

            _calculator.Calculate(3000, loan, startDate);
            var amountDue = loan.Schedule[0].AmountDue;

            Console.WriteLine(loan);

            var nextPayment = NextPayment(loan, new DateTime(2012, 2, 1));

            Assert.That(nextPayment, Is.EqualTo(amountDue));
        }

        [Test]
        public void payment_five_days_after_installment()
        {
            var loan = new Loan();
            var startDate = new DateTime(2012, 1, 1);

            _calculator.Calculate(3000, loan, startDate);

            Console.WriteLine(loan);

            var nextPayment = NextPayment(loan, new DateTime(2012, 2, 6));

            Assert.That(nextPayment, Is.EqualTo(1211.03m));
        }

        private static void MakeEarlyPayment(Loan loan, decimal amount, DateTime startDate)
        {
            var facade = new LoanPaymentFacade();
            facade.PayLoan(loan, "", amount, "", startDate);
        }

        private static decimal NextPayment(Loan loan , DateTime date)
        {
			var calc = new LoanRepaymentScheduleCalculator(loan, date, 0);
            return calc.NextEarlyPayment();
        }
    }
}