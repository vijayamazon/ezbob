using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    public class InterestOnlyPaymentsFixture : LoanPaymentsTestBase
    {
        /// <summary>
        /// Open questions:
        ///  - if customer has late payments
        ///  - if customer has a roll over
        /// </summary>
        [Test]
        public void pays_second_installment_only_interest()
        {
            const int amount = 5000;
            const decimal interest = 5000.0m * 0.06m;

            CreateHalfWayLoan(Parse("2012-09-23 23:29:35.000"), amount);
            
            MakePayment(interest, _loan.Schedule[0].Date);  //pay first installment ontime
            MakePaymentIO(interest, _loan.Schedule[0].Date.AddDays(5)); // pay next interest only installment

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidOnTime));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
        }
        
        [Test]
        public void next_interest_payment_recalculated_after_each_payment()
        {
            const int amount = 5000;
            const decimal interest = 5000.0m * 0.06m;

            CreateHalfWayLoan(Parse("2012-09-23 23:29:35.000"), amount);

            Assert.That(_loan.NextInterestPayment, Is.EqualTo(interest));

            MakePaymentIO(interest, _loan.Schedule[0].Date);
            Assert.That(_loan.NextInterestPayment, Is.EqualTo(interest));

            MakePaymentIO(interest, _loan.Schedule[0].Date);
            Assert.That(_loan.NextInterestPayment, Is.EqualTo(interest));

            MakePaymentIO(interest, _loan.Schedule[0].Date);
            Assert.That(_loan.NextInterestPayment, Is.EqualTo(0));
        }

        [Test]
        public void mixing_interest_only_payments_with_normal()
        {
            const int amount = 5000;
            const decimal interest = 5000.0m * 0.06m;

            CreateHalfWayLoan(Parse("2012-09-23 23:29:35.000"), amount);

            MakePaymentIO(interest, _loan.Schedule[0].Date);
            MakePaymentIO(interest, _loan.Schedule[0].Date);
            MakePayment(1000, _loan.Schedule[0].Date);

            Assert.That(_loan.Schedule[1].Interest, Is.EqualTo(0));
        }

        [Test]
        public void after_second_interest_only_repayment_schedule_is_not_broken()
        {
            const int amount = 5000;
            const decimal interest = 5000.0m * 0.06m;

            CreateHalfWayLoan(Parse("2012-09-23 23:29:35.000"), amount);
            
            MakePaymentIO(interest, _loan.Schedule[0].Date);
            MakePaymentIO(interest, _loan.Schedule[0].Date);

            Assert.That(_loan.Schedule[0].Interest, Is.EqualTo(0));
            Assert.That(_loan.Schedule[0].AmountDue, Is.EqualTo(0));
            Assert.That(_loan.Schedule[0].LoanRepayment, Is.EqualTo(0));

            Assert.That(_loan.Schedule[1].Interest, Is.EqualTo(0));
            Assert.That(_loan.Schedule[1].AmountDue, Is.EqualTo(0));
            Assert.That(_loan.Schedule[1].LoanRepayment, Is.EqualTo(0));

        }


        protected void MakePaymentIO(decimal amount, DateTime date)
        {
            Console.WriteLine("Making interest only payment {0} on {1}", amount, date);
            _facade.PayLoan(_loan, "", amount, "", date, "", true);
            Console.WriteLine(_loan);
        }

    }
}