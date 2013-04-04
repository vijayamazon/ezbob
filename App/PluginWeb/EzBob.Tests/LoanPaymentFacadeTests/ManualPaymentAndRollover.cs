using System;
using EZBob.DatabaseLib.Model.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    public class ManualPaymentAndRollover : LoanPaymentsTestBase
    {
        [Test]
        public void loan_276()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(1475, _loan, Parse("2012-12-11 17:07:51.000"));

            Console.WriteLine(_loan);

            MakePayment(123, Parse("2012-12-19 00:00:00.000"));

            var rollover = new PaymentRollover()
                            {
                                LoanSchedule = _loan.Schedule[0],
                                Created = Parse("2012-12-19 16:20:35.000"),
                                ExpiryDate = Parse("2012-12-22 00:00:00.000"),
                                Payment = 57
                            };

            _loan.Schedule[0].Rollovers.Add(rollover);

            _facade.GetStateAt(_loan, Parse("2012-12-19 16:20:35.000"));

            Console.WriteLine(_loan);

            Assert.That(rollover.Payment, Is.EqualTo(57m));
        }
    }
}