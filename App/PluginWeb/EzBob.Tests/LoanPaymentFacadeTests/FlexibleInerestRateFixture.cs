using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    public class FlexibleInerestRateFixture : LoanPaymentsTestBase
    {
        [Test]
        public void can_recalculate_simple_flexible_ir_loan()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.2M, Term = 2};
            calculator.Calculate(1000, _loan, Parse("2012-01-01 12:00:00.000"));

            _loan.Schedule[1].InterestRate = 0.1M;

            _facade.GetStateAt(_loan, _loan.Date);

            Assert.That(_loan.Schedule[0].Interest, Is.EqualTo(200));
            Assert.That(_loan.Schedule[1].Interest, Is.EqualTo(50));
        }
    }
}