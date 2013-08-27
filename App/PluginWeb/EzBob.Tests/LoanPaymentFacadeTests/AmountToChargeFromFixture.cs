using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    public class AmountToChargeFromFixture : LoanPaymentsTestBase
    {
        [Test]
        public void after_paying_5_poundsless_than_installment_loan_is_not_late()
        {
            CreateLoan(Parse("2012-01-01 12:00:00.000"), 1000);
            MakePayment(345, Parse("2012-02-02 12:00:00.000"));

            Console.WriteLine(_loan);

            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Live));
        }
    }
}