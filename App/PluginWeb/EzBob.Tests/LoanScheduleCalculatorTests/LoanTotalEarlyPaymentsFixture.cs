using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests
{
    [TestFixture]
    public class LoanTotalEarlyPaymentsFixture
    {
        private LoanScheduleCalculator _calculator;

        [SetUp]
        public void SetUp()
        {
            _calculator = new LoanScheduleCalculator();
        }

        [Test]
        public void loan_of_3k_pay_in_the_middle()
        {
            var loan = new Loan() {};
            _calculator.Calculate(3000, loan, new DateTime(2012, 1, 1));

            var totalEarlyPayment = TotalEarlyPayment(loan, new DateTime(2012, 1, 15));

            Assert.That(totalEarlyPayment, Is.EqualTo(3081.29));
        }

        [Test]
        public void loan_of_3k_pay_after_first_installment()
        {
            var loan = new Loan() {};
            _calculator.Calculate(3000, loan, new DateTime(2012, 1, 1));

            var totalEarlyPayment = TotalEarlyPayment(loan, new DateTime(2012, 2, 15));

            Assert.That(totalEarlyPayment, Is.EqualTo(3266.9m));
        }

        [Test]
        public void loan_of_3k_payment_after_last_installment()
        {
            var loan = new Loan() {};
            _calculator.Calculate(3000, loan, new DateTime(2012, 1, 1));

            var totalEarlyPayment = TotalEarlyPayment(loan, new DateTime(2012, 4, 15));

            Assert.That(totalEarlyPayment, Is.EqualTo(3624m));
        }

        [Test]
        public void loan_of_600_payment_first_day_totatl_and_installment()
        {
            var loan = new Loan() {};
            var date = new DateTime(2012, 12, 18);
            _calculator.Calculate(600, loan, date);

			var c = new LoanRepaymentScheduleCalculator(loan, date, 0);
            
            Console.WriteLine(loan);

            var next = c.NextEarlyPayment();

            Console.WriteLine(loan);

            var totalEarlyPayment = c.TotalEarlyPayment();

            Console.WriteLine(loan);
            
            Assert.That(next, Is.EqualTo(200m));
            Assert.That(totalEarlyPayment, Is.EqualTo(600m));

            Assert.That(loan.Schedule.All(i => i.Interest > 0), Is.True);
        }

        private decimal TotalEarlyPayment(Loan loan, DateTime dateTime)
        {
			var c = new LoanRepaymentScheduleCalculator(loan, dateTime, 0);
            var totalEarlyPayment = c.TotalEarlyPayment();
            return totalEarlyPayment;
        }

        [Test]
        public void loan_of_3k_paytoday()
        {
            var loan = new Loan() {};
            _calculator.Calculate(3000, loan, new DateTime(2012, 1, 1));

            var totalEarlyPayment = TotalEarlyPayment(loan, new DateTime(2012, 1, 1));

            Assert.That(totalEarlyPayment, Is.EqualTo(3000));
        }

        [Test]
        public void loan_with_fee()
        {
            var loan = new Loan() {};

            _calculator.Term = 6;
            _calculator.Calculate(1000, loan, new DateTime(2013, 05, 10));

            loan.Charges.Add(new LoanCharge() { Amount = 121, Loan = loan, Date = new DateTime(2013, 6, 27)});

            var totalEarlyPayment = TotalEarlyPayment(loan, new DateTime(2013, 6, 27));

            Assert.That(totalEarlyPayment, Is.EqualTo(1000+121+94m));
        }
    }
}