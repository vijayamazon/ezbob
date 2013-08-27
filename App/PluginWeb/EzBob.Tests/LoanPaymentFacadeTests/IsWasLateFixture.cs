using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    public class IsWasLateFixture : LoanPaymentsTestBase
    {
        [Test]
        public void if_payment_is_late_set_iswaslate()
        {
            CreateLoan(Parse("2012-01-01 12:00:00.000"), 1000);

            MakePayment(100, Parse("2012-03-01 12:00:00.000"));

            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Late));
            Assert.That(_loan.Customer.IsWasLate, Is.True);

        }

        [Test]
        public void if_payment_is_late_then_paid_set_iswaslate()
        {
            CreateLoan(Parse("2012-01-01 12:00:00.000"), 1000);

            MakePayment(100, Parse("2012-03-01 12:00:00.000"));
            MakePayment(900, Parse("2012-03-02 12:00:00.000"));

            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Live));
            Assert.That(_loan.Customer.IsWasLate, Is.True);

        }
    }
}