using System;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Models;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    public class DelinquencyFixture : LoanPaymentsTestBase
    {
        [Test]
        [Ignore]
        public void delinquency_for_one_missed_installment()
        {
            CreateLoan(Parse("2012-01-01 12:00:00.000"), 1000);
            MakePayment(389, Parse("2012-02-15 12:00:00.000"));

            Assert.That(_loan.MaxDelinquencyDays, Is.EqualTo(14));
            Assert.That(_loan.Customer.CreditResult, Is.EqualTo(CreditResultStatus.Late));

        }

        [Test]
        [Ignore]
        public void delinquency_for_almost_paid_installment()
        {
            CreateLoan(Parse("2013-07-30 00:00:00.000"), 1000);
            MakePayment(235.0m, Parse("2013-08-30 00:00:00.000"));
            GetStateAt(_loan, Parse("2013-09-04 00:00:00.000"));

            Console.Write(_loan);

            Assert.That(_loan.MaxDelinquencyDays, Is.EqualTo(5));
            Assert.That(_loan.Customer.CreditResult, Is.EqualTo(CreditResultStatus.Late));
        }

        [Test]
        [Ignore]
        public void delinquency_for_almost_paid_installment2()
        {
            CreateLoan(Parse("2013-07-25 00:00:00.000"), 1000);
            MakePayment(235.0m, Parse("2013-08-25 00:00:00.000"));
            GetStateAt(_loan, Parse("2013-08-30 00:00:00.000"));

            Console.Write(_loan);

            Assert.That(_loan.MaxDelinquencyDays, Is.EqualTo(5));
            Assert.That(_loan.Customer.CreditResult, Is.EqualTo(CreditResultStatus.Late));
        }

        [Test]
        [Ignore]
        public void delinquency_for_almost_paid_installment3()
        {
            CreateLoan(Parse("2013-07-25 00:00:00.000"), 1000);
            MakePayment(235.0m, Parse("2013-08-25 00:00:00.000"));

            var p = new LoanRepaymentScheduleCalculator(_loan, Parse("2013-08-30 00:00:00.000"), 0).NextEarlyPayment();

            Console.Write(_loan);

            Assert.That(_loan.MaxDelinquencyDays, Is.EqualTo(5));
            Assert.That(_loan.Customer.CreditResult, Is.EqualTo(CreditResultStatus.Late));
        }

        [Test]
        [Ignore]
        public void delinquency_for_almost_paid_installment4()
        {
            _calculator.Interest = 0.069m;
            CreateLoan(Parse("2013-07-25 00:00:00.000"), 1000);
            MakePayment(236.0m, Parse("2013-08-25 00:00:00.000"));
            MakePayment(164.0m, Parse("2013-08-25 00:00:00.000"));

            LoanModel.FromLoan(_loan, new LoanRepaymentScheduleCalculator(_loan, Parse("2013-08-30 16:52:00.000"), 0));

            Console.Write(_loan);

            Assert.That(_loan.MaxDelinquencyDays, Is.EqualTo(5));
            Assert.That(_loan.Customer.CreditResult, Is.EqualTo(CreditResultStatus.Late));
        }

        protected override int GetAmountToChargeFrom()
        {
            return 10;
        }
    }
}