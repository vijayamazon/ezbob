using System;
using EZBob.DatabaseLib.Model.Database;
using NUnit.Framework;

namespace EzBob.Tests.LoanCreatorTests
{
    public class LoanCreatorFixture : LoanCreatorFixtureBase
    {
        private Customer _customer;

        public override void SetUp()
        {
            _customer = new Customer()
            {
                PersonalInfo = new PersonalInfo() { FirstName = "Test" },
                BankAccount = new BankAccount(),
                Status = Status.Approved,
                CollectionStatus = new CollectionStatus { CurrentStatus = CollectionStatusType.Enabled },
                CreditSum = 10000,
                OfferStart = DateTime.UtcNow.AddDays(-1),
                OfferValidUntil = DateTime.UtcNow.AddDays(1),
                IsSuccessfullyRegistered = true
            };
        }

        [Test]
        public void create_loan_without_template()
        {
            var cr = new CashRequest()
                         {
                             InterestRate = 0.06M,
                             RepaymentPeriod = 3
                         };


            _customer.CashRequests.Add(cr);

            var loan = _lc.CreateLoan(_customer, 10000, null, new DateTime(2013, 10, 21));

            Assert.That(loan.Schedule.Count, Is.EqualTo(3));
            Assert.That(loan.LoanAmount, Is.EqualTo(10000));
        }

        [Test]
        public void create_loan_with_template()
        {
            var cr = new CashRequest()
                         {
                             InterestRate = 0.06M,
                             RepaymentPeriod = 6
                         };


            var loan = _loanBuilder.CreateLoan(cr, 3000, DateTime.UtcNow);
            var model = _loanDetailsModelBuilder.BuildModel(loan);
            cr.LoanTemplate = model.ToJSON();

            _customer.CashRequests.Add(cr);

            _customer.CreditSum = 3000;

            var loan2 = _lc.CreateLoan(_customer, 10, null, new DateTime(2013, 10, 21));

            Assert.That(loan2.Schedule.Count, Is.EqualTo(6));
            Assert.That(loan.LoanAmount, Is.EqualTo(3000));
            Assert.That(loan2.Schedule[0].LoanRepayment, Is.EqualTo(500));
        }

        [Test]
        public void create_loan_with_template_and_shift_installments()
        {
            var cr = new CashRequest()
                         {
                             InterestRate = 0.06M,
                             RepaymentPeriod = 6
                         };


            var loan = _loanBuilder.CreateLoan(cr, 3000, new DateTime(2013, 10, 11));
            var model = _loanDetailsModelBuilder.BuildModel(loan);
            cr.LoanTemplate = model.ToJSON();

            var actual = _loanBuilder.CreateLoan(cr, 10, new DateTime(2013, 11, 5));

            Assert.That(actual.Schedule[0].Date, Is.EqualTo(new DateTime(2013, 12, 6)));
        }
    }
}