using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    public class GetStateAtFixture : LoanPaymentsTestBase
    {
        [Test]
        [Description("Every unpaid installment adds amount to pay")]
        public void state_at_installment_dates()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(5000, _loan, Parse("2012-09-23 23:29:35.000"));

            var charge = new LoanCharge() { Amount = 60, Loan = _loan, Date = new DateTime(2012, 10, 25) };
            _loan.Charges.Add(charge);

            Console.WriteLine(_loan);

            LoanScheduleItem state;

            //loan start
            state = _facade.GetStateAt(_loan, new DateTime(2012, 9, 23));
            Console.WriteLine(_loan);
            Assert.That(state.AmountDue, Is.EqualTo(0));
            Assert.That(state.LoanRepayment, Is.EqualTo(0));
            Assert.That(state.Interest, Is.EqualTo(0));
            Assert.That(state.Fees, Is.EqualTo(0));

            Console.WriteLine(_loan);

            // installment #1
            state = _facade.GetStateAt(_loan, new DateTime(2012, 10, 23));
            Console.WriteLine(_loan);
            Assert.That(state.AmountDue, Is.EqualTo(1968.00m));
            Assert.That(state.LoanRepayment, Is.EqualTo(1668));
            Assert.That(state.Interest, Is.EqualTo(300.00m));
            Assert.That(state.Fees, Is.EqualTo(0));

            // installment #2
            state = _facade.GetStateAt(_loan, new DateTime(2012, 11, 23));
            Console.WriteLine(_loan);
            //Assert.That(state.AmountDue, Is.EqualTo(1865.92m));
            Assert.That(state.LoanRepayment, Is.EqualTo(1666 + 1668));
            Assert.That(state.Fees, Is.EqualTo(60));
            //Assert.That(state.Interest, Is.EqualTo(199.92m));

            // installment #3
            state = _facade.GetStateAt(_loan, new DateTime(2012, 12, 23));
            Console.WriteLine(_loan);
            //Assert.That(state.AmountDue, Is.EqualTo(1765.96m));
            Assert.That(state.LoanRepayment, Is.EqualTo(1666 + 1666 + 1668));
            Assert.That(state.Fees, Is.EqualTo(60));
            //Assert.That(state.Interest, Is.EqualTo(99.96m));           
        }

        [Test]
        public void state_at_begining_of_the_loan_with_late_charge()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(5000, _loan, Parse("2012-09-23 23:29:35.000"));

            Console.WriteLine(_loan);

            var charge = new LoanCharge() { Amount = 60, Loan = _loan, Date = new DateTime(2012, 10, 25) };
            _loan.Charges.Add(charge);

            var state = _facade.GetStateAt(_loan, new DateTime(2012, 9, 23));

            Console.WriteLine(_loan);

            Assert.That(state.AmountDue, Is.EqualTo(0));
            Assert.That(state.LoanRepayment, Is.EqualTo(0));
            Assert.That(state.Interest, Is.EqualTo(0));
        }

        [Test]
        [Description("Percents are added to installment's principal")]
        public void state_after_missed_payment_is_correct()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(5000, _loan, Parse("2012-09-23 23:29:35.000"));

            Console.WriteLine(_loan);

            var fee = 60m;
            var charge = new LoanCharge() { Amount = fee, Loan = _loan, Date = new DateTime(2012, 10, 25) };
            _loan.Charges.Add(charge);

            var state = _facade.GetStateAt(_loan, new DateTime(2012, 10, 26));

            Console.WriteLine(_loan);

            var percents = 29.03m; // проценты, которые набежали с момента последнего instllment
            var oldInstallmentAmountDue = 1968m;
            Assert.That(state.AmountDue, Is.EqualTo(oldInstallmentAmountDue + fee + percents));
            //Assert.That(state.LoanRepayment, Is.EqualTo(0));
            //Assert.That(state.Interest, Is.EqualTo(0));
        }
    }
}
