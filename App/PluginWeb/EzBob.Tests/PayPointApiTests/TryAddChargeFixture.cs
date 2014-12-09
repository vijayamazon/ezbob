using System;
using EZBob.DatabaseLib.Model;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.PayPointApiTests
{
    [TestFixture]
    public class TryAddChargeFixture
    {
        [Test]
        public void charge_can_be_added()
        {
            var loan = new Loan();
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(1000, loan, new DateTime(2012, 10, 10));

            var charge = new LoanCharge
            {
                Amount = 100,
                ChargesType = new ConfigurationVariable(){Id = 1, Name = "Charge1", Value = "100"},
                Date = new DateTime(2012, 10, 11),
                Loan = loan
            };

            loan.TryAddCharge(charge);

            Assert.That(loan.Charges.Count, Is.EqualTo(1));

        }

        //LatePaymentCharge, AdministrationCharge, PartialPaymentCharge
        [Test]
        public void charges_can_be_added_to_different_installments()
        {
            var loan = new Loan();
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(1000, loan, new DateTime(2012, 10, 10));

            var charge1 = new LoanCharge
                              {
                                  Amount = 100,
                                  ChargesType =
                                      new ConfigurationVariable() {Id = 1, Name = "LatePaymentCharge", Value = "100"},
                                  Date = new DateTime(2012, 11, 11),
                                  Loan = loan
                              };

            var charge2 = new LoanCharge
                            {
                                Amount = 200,
                                ChargesType = new ConfigurationVariable() { Id = 1, Name = "LatePaymentCharge", Value = "100" },
                                Date = new DateTime(2012, 12, 11),
                                Loan = loan
                            };

            loan.TryAddCharge(charge1);
            loan.TryAddCharge(charge2);

            Assert.That(loan.Charges.Count, Is.EqualTo(2));

        }

        [Test]
        public void cannot_add_charge_of_the_same_type_to_one_installment()
        {
            var loan = new Loan();
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(1000, loan, new DateTime(2012, 10, 10));

            var charge1 = new LoanCharge
                              {
                                  Amount = 100,
                                  ChargesType =
                                      new ConfigurationVariable() {Id = 1, Name = "LatePaymentCharge", Value = "100"},
                                  Date = new DateTime(2012, 11, 11),
                                  Loan = loan
                              };

            var charge2 = new LoanCharge
                            {
                                Amount = 200,
                                ChargesType = new ConfigurationVariable() { Id = 1, Name = "LatePaymentCharge", Value = "100" },
                                Date = new DateTime(2012, 11, 11),
                                Loan = loan
                            };

            loan.TryAddCharge(charge1);
            loan.TryAddCharge(charge2);

            Assert.That(loan.Charges.Count, Is.EqualTo(1));
        }

        [Test]
        public void charge_of_different_types_can_be_added_to_one_installment()
        {
            var loan = new Loan();
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(1000, loan, new DateTime(2012, 10, 10));

            var charge1 = new LoanCharge
                              {
                                  Amount = 100,
                                  ChargesType =
                                      new ConfigurationVariable() {Id = 1, Name = "LatePaymentCharge", Value = "100"},
                                  Date = new DateTime(2012, 11, 11),
                                  Loan = loan
                              };

            var charge2 = new LoanCharge
                            {
                                Amount = 200,
                                ChargesType = new ConfigurationVariable() { Id = 1, Name = "AdministrationCharge", Value = "100" },
                                Date = new DateTime(2012, 11, 11),
                                Loan = loan
                            };

            loan.TryAddCharge(charge1);
            loan.TryAddCharge(charge2);

            Assert.That(loan.Charges.Count, Is.EqualTo(2));
        }

        [Test]
        public void admin_fee_is_partial_fee()
        {
            var loan = new Loan();
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(1000, loan, new DateTime(2012, 10, 10));

            var charge1 = new LoanCharge
                              {
                                  Amount = 100,
                                  ChargesType =
                                      new ConfigurationVariable() { Id = 1, Name = "PartialPaymentCharge", Value = "100" },
                                  Date = new DateTime(2012, 11, 11),
                                  Loan = loan
                              };

            var charge2 = new LoanCharge
                            {
                                Amount = 200,
                                ChargesType = new ConfigurationVariable() { Id = 1, Name = "AdministrationCharge", Value = "100" },
                                Date = new DateTime(2012, 11, 11),
                                Loan = loan
                            };

            loan.TryAddCharge(charge1);
            loan.TryAddCharge(charge2);

            Assert.That(loan.Charges.Count, Is.EqualTo(1));
        }
    }
}
