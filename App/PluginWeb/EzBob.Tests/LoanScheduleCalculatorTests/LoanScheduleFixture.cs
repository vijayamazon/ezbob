using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests
{
    [TestFixture]
    public class LoanScheduleFixture
    {
        private LoanScheduleCalculator _calculator;

        [SetUp]
        public void SetUp()
        {
            _calculator = new LoanScheduleCalculator();
        }

        [Test]
        public void simple_loan()
        {
            var schedule = _calculator.Calculate(3000m);
            Assert.That(schedule.Count, Is.EqualTo(3));
            
            Assert.That(schedule[0].AmountDue, Is.EqualTo(1180));
            Assert.That(schedule[0].Balance, Is.EqualTo(2000));
            Assert.That(schedule[0].Interest, Is.EqualTo(180));
            Assert.That(schedule[0].LoanRepayment, Is.EqualTo(1000));

            Assert.That(schedule[1].AmountDue, Is.EqualTo(1120));
            Assert.That(schedule[1].Balance, Is.EqualTo(1000));
            Assert.That(schedule[1].Interest, Is.EqualTo(120));
            Assert.That(schedule[1].LoanRepayment, Is.EqualTo(1000));

            Assert.That(schedule[2].AmountDue, Is.EqualTo(1060));
            Assert.That(schedule[2].Balance, Is.EqualTo(0));
            Assert.That(schedule[2].Interest, Is.EqualTo(60));
            Assert.That(schedule[2].LoanRepayment, Is.EqualTo(1000));
        }

        [Test]
        public void balance_is_calculated_for_loan()
        {
            var loan = new Loan();
            loan.Date = DateTime.Now;
            _calculator.Calculate(3000, loan, loan.Date);

            Assert.That(loan.Principal, Is.EqualTo(3000));
        }

        [Test]
        public void loan_has_interest_rate_and_other_fields()
        {
            var loan = new Loan();
            var startDate = new DateTime(2012, 1, 1);
            _calculator.Calculate(3000m, loan, startDate);
            Assert.That(loan.InterestRate, Is.EqualTo(_calculator.Interest));
            Assert.That(loan.LoanAmount, Is.EqualTo(3000m));
            Assert.That(loan.NextRepayment, Is.EqualTo(1180));
            Assert.That(loan.Interest, Is.EqualTo(180+120+60));
            Assert.That(loan.Balance, Is.EqualTo(loan.Interest + loan.LoanAmount));
            Assert.That(loan.Date, Is.EqualTo(startDate));
        }

        [Test]
        public void one_hundred_loan_repayments()
        {
            var schedule = _calculator.Calculate(100m);
            Assert.That(schedule.Count, Is.EqualTo(3));

            Assert.That(schedule.Sum(s => s.LoanRepayment), Is.EqualTo(100m));

            Assert.That(schedule[0].LoanRepayment , Is.EqualTo(34));
            Assert.That(schedule[1].LoanRepayment, Is.EqualTo(33));
            Assert.That(schedule[2].LoanRepayment, Is.EqualTo(33));

            Assert.That(schedule[2].Balance, Is.EqualTo(0));
        }

        [Test]
        public void one_hundred_loan_interests()
        {
            var schedule = _calculator.Calculate(100m);
            Assert.That(schedule.Count, Is.EqualTo(3));

            Assert.That(schedule[0].Interest , Is.EqualTo(6));
            Assert.That(schedule[1].Interest, Is.EqualTo(3.96));
            Assert.That(schedule[2].Interest, Is.EqualTo(1.98));

            Assert.That(schedule[2].Balance, Is.EqualTo(0));
        }

        [Test]
        public void zero_interest_loan()
        {
            _calculator.Interest = 0;
            var schedule = _calculator.Calculate(100m);
            Assert.That(schedule.Count, Is.EqualTo(3));
            Assert.That(schedule.Sum(s => s.Interest), Is.EqualTo(0));
            Assert.That(schedule.Sum(s => s.AmountDue), Is.EqualTo(100));
        }

        [Test]
        public void Recalculate()
        {
            var schedule = _calculator.Calculate(100m);

            var loan = new Loan() {Schedule = schedule};

            _calculator.Calculate(3000m, loan, loan.Date);
            Assert.That(schedule.Count, Is.EqualTo(3));

            Assert.That(schedule[0].AmountDue, Is.EqualTo(1180));
            Assert.That(schedule[0].Balance, Is.EqualTo(2000));
            Assert.That(schedule[0].Interest, Is.EqualTo(180));
            Assert.That(schedule[0].LoanRepayment, Is.EqualTo(1000));

            Assert.That(schedule[1].AmountDue, Is.EqualTo(1120));
            Assert.That(schedule[1].Balance, Is.EqualTo(1000));
            Assert.That(schedule[1].Interest, Is.EqualTo(120));
            Assert.That(schedule[1].LoanRepayment, Is.EqualTo(1000));

            Assert.That(schedule[2].AmountDue, Is.EqualTo(1060));
            Assert.That(schedule[2].Balance, Is.EqualTo(0));
            Assert.That(schedule[2].Interest, Is.EqualTo(60));
            Assert.That(schedule[2].LoanRepayment, Is.EqualTo(1000));
        }
    }
}