using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests
{
    [TestFixture]
    public class ScheduleWithSetupFee
    {
        private LoanScheduleCalculator _calculator;

        [SetUp]
        public void SetUp()
        {
            _calculator.SetUpFee = 240;
            _calculator = new LoanScheduleCalculator();
        }

        [Test]
        [Ignore]
        public void simple_loan()
        {
            var schedule = _calculator.Calculate(3000m);
            Assert.That(schedule.Count, Is.EqualTo(3));

            //Assert.That(schedule[0].AmountDue, Is.EqualTo(1180));
            //Assert.That(schedule[0].Balance, Is.EqualTo(2000));
            //Assert.That(schedule[0].Interest, Is.EqualTo(165));
            //Assert.That(schedule[0].LoanRepayment, Is.EqualTo(1000));

            //Assert.That(schedule[1].AmountDue, Is.EqualTo(1120));
            //Assert.That(schedule[1].Balance, Is.EqualTo(1000));
            //Assert.That(schedule[1].Interest, Is.EqualTo(120));
            //Assert.That(schedule[1].LoanRepayment, Is.EqualTo(1000));

            //Assert.That(schedule[2].AmountDue, Is.EqualTo(1060));
            //Assert.That(schedule[2].Balance, Is.EqualTo(0));
            //Assert.That(schedule[2].Interest, Is.EqualTo(60));
            //Assert.That(schedule[2].LoanRepayment, Is.EqualTo(1000));
        }
    }
}