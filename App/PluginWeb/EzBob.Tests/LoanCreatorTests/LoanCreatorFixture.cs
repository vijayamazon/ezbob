using System;
using System.Linq;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.CommonLib;
using NUnit.Framework;
using StructureMap;

namespace EzBob.Tests.LoanCreatorTests
{
	using DbConstants;

	public class LoanCreatorFixture : LoanCreatorFixtureBase
    {
        private Customer _customer;

        public override void SetUp()
        {
			var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;

            _customer = new Customer()
            {
                PersonalInfo = new PersonalInfo() { FirstName = "Test" },
                BankAccount = new BankAccount(),
                Status = Status.Approved,
				CollectionStatus = new CollectionStatus { CurrentStatus = new CustomerStatuses { Id = 0, Name = "Enabled", IsEnabled = true } },
                CreditSum = 10000,
                OfferStart = DateTime.UtcNow.AddDays(-1),
                OfferValidUntil = DateTime.UtcNow.AddDays(1),
				WizardStep = oDBHelper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.AllStep)
            };
        }

        [Test]
        public void create_loan_without_template()
        {
            var cr = new CashRequest()
                         {
                             InterestRate = 0.06M,
                             RepaymentPeriod = 3,
                             LoanType = new StandardLoanType()
                         };

            _customer.CashRequests.Add(cr);
            _customer.BankAccount = new BankAccount(){AccountNumber = "111111", SortCode = "222222"};

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
                             RepaymentPeriod = 6,
                             LoanType = new StandardLoanType()
                         };

            var loan = _loanBuilder.CreateLoan(cr, 3000, DateTime.UtcNow);
            var model = _loanDetailsModelBuilder.BuildModel(loan);
            cr.LoanTemplate = model.ToJSON();

            _customer.CashRequests.Add(cr);
            _customer.BankAccount = new BankAccount(){AccountNumber = "111111", SortCode = "1111111"};
            _customer.CreditSum = 3000;

            var loan2 = _lc.CreateLoan(_customer, 10, null, new DateTime(2013, 10, 21));

            Assert.That(loan2.Schedule.Count, Is.EqualTo(6));
            Assert.That(loan2.LoanAmount, Is.EqualTo(10));
            Assert.That(loan2.Schedule[0].LoanRepayment, Is.EqualTo(5));
        }

        [Test]
        public void create_loan_with_template_and_shift_installments()
        {
            var cr = new CashRequest()
                         {
                             InterestRate = 0.06M,
                             RepaymentPeriod = 6,
                             LoanType = new StandardLoanType()
                         };

            var loan = _loanBuilder.CreateLoan(cr, 3000, new DateTime(2013, 10, 11));
            var model = _loanDetailsModelBuilder.BuildModel(loan);
            cr.LoanTemplate = model.ToJSON();

            var actual = _loanBuilder.CreateLoan(cr, 10, new DateTime(2013, 11, 5));

            Assert.That(actual.Schedule[0].Date, Is.EqualTo(new DateTime(2013, 12, 6)));
        }
    }
}
