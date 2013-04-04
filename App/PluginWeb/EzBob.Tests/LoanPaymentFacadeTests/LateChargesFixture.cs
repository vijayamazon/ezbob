using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    public class LateChargesFixture : LoanPaymentsTestBase
    {
        [Test]
        [Description("add late charge after missed payment")]
        public void simple_late_charge()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(5000, _loan, Parse("2012-09-23 23:29:35.000"));

            var charge = new LoanCharge(){Amount = 60, Loan = _loan, Date = new DateTime(2012, 10, 25)};
            _loan.Charges.Add(charge);

            Console.WriteLine(_loan);

            MakePayment(1968,     Parse("2012-10-28 10:03:01.000"));
            MakePayment(1865.92m, Parse("2012-11-23 10:44:22.000"));
            MakePayment(1765.96m, Parse("2012-12-23 13:30:18.000"));

            Assert.That(_loan.TransactionsWithPaypointSuccesefull[0].Fees, Is.EqualTo(60));
            Assert.That(charge.AmountPaid, Is.EqualTo(60));
        }

        [Test]
        [Description("late charges affects early payments")]
        public void early_payment_with_late_charge()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(5000, _loan, Parse("2012-09-23 23:29:35.000"));

            Console.WriteLine(_loan);

            var charge = new LoanCharge(){Amount = 60, Loan = _loan, Date = new DateTime(2012, 10, 25)};
            _loan.Charges.Add(charge);

            var payment = _loan.TotalEarlyPayment(_loan.Date);

            Console.WriteLine(_loan);

            Assert.That(payment, Is.EqualTo(_loan.LoanAmount));
        }

        [Test]
        [Description("late charge before first installment")]
        public void late_charge_before_first_installment()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(5000, _loan, Parse("2012-09-23 23:29:35.000"));

            var charge = new LoanCharge(){Amount = 60, Loan = _loan, Date = new DateTime(2012, 09, 30)};
            _loan.Charges.Add(charge);

            var payment = _loan.TotalEarlyPayment(_loan.Date);

            Console.WriteLine(_loan);

            Assert.That(_loan.Schedule[0].Fees, Is.EqualTo(60));
        }

        [Test]
        [Description("late charge after first installment, that is paid")]
        public void late_charge_after_first_installment_that_is_paid()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(5000, _loan, Parse("2012-09-23 23:29:35.000"));

            var charge = new LoanCharge(){Amount = 60, Loan = _loan, Date = new DateTime(2012, 10, 30)};
            _loan.Charges.Add(charge);

            _facade.PayLoan(_loan, "", 3000, "", new DateTime(2012, 10, 23));

            Console.WriteLine(_loan);

            Assert.That(_loan.Schedule[0].Fees, Is.EqualTo(0));
            Assert.That(_loan.Schedule[1].Fees, Is.EqualTo(60));
        }

        [Test]
        [Description("charge after installment adds to it or not depending on current date")]
        public void add_or_not_charge_after_installment()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(5000, _loan, Parse("2012-09-23 23:29:35.000"));

            var charge = new LoanCharge(){Amount = 60, Loan = _loan, Date = new DateTime(2012, 11, 30)};
            _loan.Charges.Add(charge);

            _facade.Recalculate(_loan, Parse("2012-10-23 23:29:35.000"));

            Console.WriteLine(_loan);

            Assert.That(_loan.Schedule.Sum(x => x.Fees), Is.EqualTo(60));
            Assert.That(_loan.Schedule[2].Fees, Is.EqualTo(60));
        }
    }
}