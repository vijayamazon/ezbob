using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests
{
    [TestFixture]
    public class PaymentCalculatorFixture
    {

        private LoanScheduleCalculator _calculator;
        private IList<LoanScheduleItem> _schedule;
        private DateTime _startDate;
        private Loan _loan;
        private LoanPaymentFacade _facade;

        [SetUp]
        public void SetUp()
        {
            _facade = new LoanPaymentFacade();
            _calculator = new LoanScheduleCalculator();
            _loan = new Loan();
            _startDate = new DateTime(2012, 10, 10);
            _schedule = _calculator.Calculate(3000m, _loan, _startDate);
        }

        [Test]
        [Description("Pay exact amount of installment")]
        public void exact_amount_pays_installment()
        {
            var installment = _schedule.First();

            var amountDue = installment.AmountDue;

            Console.WriteLine(_loan);

            PayInstallment(amountDue, installment, installment.Date);

            Console.WriteLine(_loan);

            Assert.That(installment.AmountDue, Is.EqualTo(0));
            Assert.That(installment.Interest, Is.EqualTo(0));
            Assert.That(installment.Fees, Is.EqualTo(0));
            Assert.That(installment.LateCharges, Is.EqualTo(0));
            //Assert.That(installment.RepaymentAmount, Is.EqualTo(amountDue));

            _loan.UpdateStatus(installment.Date);
            _loan.UpdateBalance();

            Assert.That(_loan.Repayments, Is.EqualTo(amountDue));
        }

        private void PayInstallment(decimal amountDue, LoanScheduleItem installment, DateTime date)
        {
            _facade.PayLoan(_loan, "", amountDue, "", date);
        }

        [Test]
        [Description("Pay exact amount of installment in two attempts")]
        public void exact_amount_pays_installment_in_two_attempts()
        {
            var installment = _schedule.First();

            var amountDue = installment.AmountDue;

            PayInstallment(amountDue / 2, installment, installment.Date);
            PayInstallment(amountDue / 2, installment, installment.Date);

            AssertInstallmentIsClosed(installment);

            _loan.UpdateStatus(installment.Date);
            _loan.UpdateBalance();

            Assert.That(_loan.Repayments, Is.EqualTo(amountDue));
        }

        private static void AssertInstallmentIsClosed(LoanScheduleItem installment)
        {
            Assert.That(installment.AmountDue, Is.EqualTo(0));
            Assert.That(installment.Interest, Is.EqualTo(0));
            Assert.That(installment.LoanRepayment, Is.EqualTo(0));
            Assert.That(installment.Fees, Is.EqualTo(0));
            Assert.That(installment.LateCharges, Is.EqualTo(0));
        }

        [Test]
        [Description("Pay exact amount of installment in two attempts. The first attempt was bigger than second")]
        public void exact_amount_pays_installment_in_two_not_equal_attempts()
        {
            var installment = _schedule.First();

            var amountDue = installment.AmountDue;

            PayInstallment(amountDue * 0.8M, installment, installment.Date);
            PayInstallment(amountDue * 0.2M, installment, installment.Date);

            AssertInstallmentIsClosed(installment);

            _loan.UpdateStatus(installment.Date);
            _loan.UpdateBalance();

            Assert.That(_loan.Repayments, Is.EqualTo(amountDue));
        }
        
        [Test]
        [Description("pay zero. nothing should be affected")]
        public void zero_does_not_pay_anything()
        {
            var installment = _schedule.First();
            var oldItem = installment.Clone();

            PayInstallment(0, installment, installment.Date);

            Assert.That(installment.AmountDue, Is.EqualTo(oldItem.AmountDue));
            Assert.That(installment.Interest, Is.EqualTo(oldItem.Interest));
            Assert.That(installment.Fees, Is.EqualTo(oldItem.Fees));
            Assert.That(installment.LateCharges, Is.EqualTo(oldItem.LateCharges));
        }

        [Test]
        [Description("paying small amount should decrease first late charges, fees, interest and then loan")]
        [Ignore]
        public void small_amount_pays_interest_first_no_fees_no_charges()
        {
            var installment = _schedule.First();
            var oldItem = installment.Clone();

            var amount = installment.Interest/2;
            PayInstallment(amount, installment, installment.Date);

            Assert.That(installment.AmountDue, Is.EqualTo(oldItem.AmountDue - amount));
            Assert.That(installment.Interest, Is.EqualTo(oldItem.Interest / 2));
            Assert.That(installment.FeesPaid, Is.EqualTo(oldItem.Fees));
            Assert.That(installment.LateCharges, Is.EqualTo(oldItem.LateCharges));
        }
    }
}