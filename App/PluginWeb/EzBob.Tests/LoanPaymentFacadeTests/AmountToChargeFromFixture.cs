using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    public class AmountToChargeFromFixture : LoanPaymentsTestBase
    {
        [Test]
        public void keep_2pounds_on_last_schedule_status_not_almostPaid()
        {
            CreateLoan(Parse("2012-01-01 12:00:00.000"), 1000);

            MakePayment(394, Parse("2012-02-02 12:00:00.000"));
            MakePayment(373, Parse("2012-03-02 12:00:00.000"));
            MakePayment(350, Parse("2012-04-02 12:00:00.000"));

            GetStateAt(_loan, Parse("2012-04-12 12:00:00.000"));

            Console.WriteLine(_loan);

            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Late));
            Assert.That(_loan.Schedule[2].Status, Is.Not.EqualTo(LoanScheduleStatus.AlmostPaid));
        }

        [Test]
        public void keep_2pounds_on_last_schedule_and_pay_last_schedule_early_status_not_almostPaid()
        {
            CreateLoan(Parse("2012-01-01 12:00:00.000"), 1000);

            MakePayment(394, Parse("2012-02-02 12:00:00.000"));
            MakePayment(373, Parse("2012-03-02 12:00:00.000"));
            MakePayment(350, Parse("2012-03-30 12:00:00.000"));

            GetStateAt(_loan, Parse("2012-04-12 12:00:00.000"));

            Console.WriteLine(_loan);

            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Late));
            Assert.That(_loan.Schedule[2].Status, Is.Not.EqualTo(LoanScheduleStatus.AlmostPaid));
        }

        [Test]
        public void after_paying_5_poundsless_than_installment_loan_is_not_late()
        {
            CreateLoan(Parse("2012-01-01 12:00:00.000"), 1000);
            MakePayment(389, Parse("2012-02-02 12:00:00.000"));

            Console.WriteLine(_loan);

            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Live));
            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.AlmostPaid));
        }

        [Test]
        [Description("paying almost all installment before due date, and checkin that it is almost paid after due date")]
        public void after_paying_5_poundsless_than_installment_loan_is_not_late2()
        {
            CreateLoan(Parse("2012-01-01 12:00:00.000"), 1000);
            MakePayment(389, Parse("2012-01-30 12:00:00.000"));
            GetStateAt(_loan, Parse("2012-02-02 12:00:00.000"));

            Console.WriteLine(_loan);

            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Live));
            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.AlmostPaid));
        }

        [Test]
        public void customer_pays_almost_all_installment_then_small_late_and_repays_it()
        {
            CreateLoan(Parse("2012-01-01 12:00:00.000"), 1000);
            MakePayment(385, Parse("2012-01-30 12:00:00.000"));
            MakePayment(100, Parse("2012-02-02 12:00:00.000"));

            Console.WriteLine(_loan);

            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Live));
            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidOnTime));
        }

        [Test]
        public void after_paying_5_pounds_less_but_befor_than_installment_loan_is_not_late()
        {
            CreateLoan(Parse("2012-01-01 12:00:00.000"), 1000);
            MakePayment(385, Parse("2012-01-30 12:00:00.000"));

            GetStateAt(_loan, Parse("2012-02-02 12:00:00.000"));

            Console.WriteLine(_loan);

            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Live));
        }

        [Test]
        public void after_paying_15_poundsless_than_installment_loan_is_late()
        {
            CreateLoan(Parse("2012-01-01 12:00:00.000"), 1000);

            Console.WriteLine(_loan);

            MakePayment(350, Parse("2012-02-02 12:00:00.000"));

            Console.WriteLine(_loan);

            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Late));
        }

        [Test]
        public void if_payment_is_late_only_for_10_pounds_do_not_set_iswaslate()
        {
            CreateLoan(Parse("2012-01-01 12:00:00.000"), 1000);

            MakePayment(389, Parse("2012-02-02 12:00:00.000"));

            Assert.That(_loan.Customer.IsWasLate, Is.False);
        }

        [Test]
        public void if_payment_is_late_only_for_10_pounds_do_not_set_iswaslate_before()
        {
            CreateLoan(Parse("2012-01-01 12:00:00.000"), 1000);

            MakePayment(385, Parse("2012-01-30 12:00:00.000"));

            GetStateAt(_loan, Parse("2012-02-02 12:00:00.000"));

            Assert.That(_loan.Customer.IsWasLate, Is.False);
        }

        [Test]
        public void if_payment_is_late_more_than_10_pounds_do_set_iswaslate()
        {
            CreateLoan(Parse("2012-01-01 12:00:00.000"), 1000);

            MakePayment(350, Parse("2012-02-02 12:00:00.000"));

            Assert.That(_loan.Customer.IsWasLate, Is.True);
        }

        protected override int GetAmountToChargeFrom()
        {
            return 10;
        }

    }
}
