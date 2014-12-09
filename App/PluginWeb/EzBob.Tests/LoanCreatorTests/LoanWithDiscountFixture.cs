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

	public class LoanWithDiscountFixture : LoanCreatorFixtureBase
    {

        private Customer _customer;

        public override void SetUp()
        {
			var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;

            _customer = new Customer()
            {
                PersonalInfo = new PersonalInfo() { FirstName = "Test" },
                BankAccount = new BankAccount() { AccountNumber = "111111", SortCode = "1111111" },
                Status = Status.Approved,
                CollectionStatus = new CollectionStatus { CurrentStatus = new CustomerStatuses { Id = 0, Name = "Enabled", IsEnabled = true } },
                CreditSum = 10000,
                OfferStart = DateTime.UtcNow.AddDays(-1),
                OfferValidUntil = DateTime.UtcNow.AddDays(1),
				WizardStep = oDBHelper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.AllStep)
            };
        }

        [Test]
        public void create_loan_with_template()
        {
            var cr = new CashRequest()
            {
                InterestRate = 0.06M,
                RepaymentPeriod = 6,
                LoanType = new StandardLoanType(),
                DiscountPlan = new DiscountPlan()
                                   {
                                       ValuesStr = "+0, -10, -20"
                                   }
            };

            var loan = _loanBuilder.CreateLoan(cr, 3000, DateTime.UtcNow);

            Assert.That(loan.Schedule[0].InterestRate * 100m, Is.EqualTo(6m));
            Assert.That(loan.Schedule[1].InterestRate * 100m, Is.EqualTo(6m - 0.6m));
            Assert.That(loan.Schedule[2].InterestRate * 100m, Is.EqualTo(6m - 1.2m));
            Assert.That(loan.Schedule[3].InterestRate * 100m, Is.EqualTo(6m));
            Assert.That(loan.Schedule[4].InterestRate * 100m, Is.EqualTo(6m));
            Assert.That(loan.Schedule[5].InterestRate * 100m, Is.EqualTo(6m));
        } 
    }
}
