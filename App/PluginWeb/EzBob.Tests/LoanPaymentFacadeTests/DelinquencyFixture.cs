using EZBob.DatabaseLib.Model.Database;
using NUnit.Framework;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    public class DelinquencyFixture : LoanPaymentsTestBase
    {
        [Test]
        public void delinquency_for_one_missed_installment()
        {
            CreateLoan(Parse("2012-01-01 12:00:00.000"), 1000);
            MakePayment(389, Parse("2012-02-15 12:00:00.000"));

            Assert.That(_loan.MaxDelinquencyDays, Is.EqualTo(14));

            Assert.That(_loan.Customer.CreditResult, Is.EqualTo(CreditResultStatus.Late));

        }
    }
}