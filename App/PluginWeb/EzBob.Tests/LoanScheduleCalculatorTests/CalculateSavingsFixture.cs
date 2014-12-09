using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests
{
    [TestFixture]
    public class CalculateSavingsFixture
    {
        private LoanScheduleCalculator _calculator;
        private LoanPaymentFacade _facade;
        private DateTime _term;

        [SetUp]
        public void SetUp()
        {
            _calculator = new LoanScheduleCalculator();
            _facade = new LoanPaymentFacade();
            _term = DateTime.UtcNow;
        }

        [Test]
        public void calculate_savings_for_one_loan()
        {
            var loan = new Loan();
            var schedule = _calculator.Calculate(3000m, loan);
            loan.Status = LoanStatus.Live;

            var customer = new Customer();
            customer.Loans.Add(loan);

            var saved = _facade.CalculateSavings(customer, _term);

            Assert.That(saved, Is.EqualTo(schedule.Sum(i => i.Interest)));
        }

        [Test]
        public void calculate_savings_for_two_loans()
        {
            var loan = new Loan();
            _calculator.Calculate(3000m, loan);
            loan.Status = LoanStatus.Live;

            var loan2 = new Loan();
            _calculator.Calculate(3000m, loan2);
            loan2.Status = LoanStatus.Live;

            var customer = new Customer();
            customer.Loans.Add(loan);
            customer.Loans.Add(loan2);

            var saved = _facade.CalculateSavings(customer, _term);

            Assert.That(saved, Is.EqualTo(loan.Schedule.Sum(i => i.Interest) + loan2.Schedule.Sum(i => i.Interest)));
        }

        [Test]
        public void calculate_savings_for_empty_customer()
        {
            var customer = new Customer();

            var saved = _facade.CalculateSavings(customer, _term);          

            Assert.That(saved, Is.EqualTo(0));
        }
    }
}
