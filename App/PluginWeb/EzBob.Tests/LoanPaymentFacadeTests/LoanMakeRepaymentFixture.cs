using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    [TestFixture]
    public class LoanMakeRepaymentFixture
    {
        private Loan _loan;
        private decimal _takenMoney = 3000;
        private LoanPaymentFacade _facade;
        private DateTime _startDate;

        [SetUp]
        public void SetUp()
        {
            _loan = new Loan();
            var calculator = new LoanScheduleCalculator();
            _startDate = new DateTime(2012, 1, 1);
            calculator.Calculate(_takenMoney, _loan, _startDate);
            _facade = new LoanPaymentFacade();
        }

        [Test]
        public void make_little_payment_decreases_loan_amount_due()
        {
            var oldAmountDue = _loan.Balance - _loan.Interest;

            Console.WriteLine(_loan);

            _facade.PayLoan(_loan, "", 100, "", _startDate);

            Console.WriteLine(_loan);

            Assert.That(_loan.Balance - _loan.Interest, Is.EqualTo(oldAmountDue - 100));
        }

        [Test]
        public void make_little_payment_recalculates_schedule()
        {
            _facade.PayLoan(_loan, "", 100, "", _startDate);

            Assert.That(_loan.Schedule[0].LoanRepayment, Is.EqualTo(1000-100));
        }

        [Test]
        public void make_little_payment_after_month_recalculates_schedule()
        {
            var oldAmountDue = _loan.Schedule[0].AmountDue;

            _loan.Schedule[0].Status = LoanScheduleStatus.Late;

            _facade.PayLoan(_loan, "", 100, "", _loan.Date.AddMonths(1));

            Assert.That(_loan.Schedule[0].AmountDue, Is.EqualTo(oldAmountDue - 100));
            //Assert.That(_loan.Schedule[0].RepaymentAmount, Is.EqualTo(100));
        }
    }
}