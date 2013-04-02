using System;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
using EzBob.Web.ApplicationCreator;
using EzBob.Web.Areas.Customer.Controllers;
using EzBob.Web.Areas.Underwriter.Controllers;
using EzBob.Web.Code;
using EzBob.Web.Code.Agreements;
using EzBob.Web.Infrastructure;
using Moq;
using NUnit.Framework;
using PaymentServices.PacNet;
using ZohoCRM;

namespace EzBob.Tests.LoanCreatorTests
{
    [TestFixture]
    public class LoanCreatorFixture
    {
        private LoanCreator _lc;
        private LoanBuilder _loanBuilder;
        private ChangeLoanDetailsModelBuilder _loanDetailsModelBuilder;
        private Customer _customer;

        [SetUp]
        public void SetUp()
        {
            var logRepository = new Mock<IPacnetPaypointServiceLogRepository>();
            var loanHistoryRepository = new Mock<ILoanHistoryRepository>();
            var pacnetService = new Mock<IPacnetService>();
            var appCreator = new Mock<IAppCreator>();
            var crm = new Mock<IZohoFacade>();
            var agreementsGenerator = new Mock<IAgreementsGenerator>();
            var context = new Mock<IEzbobWorkplaceContext>();
            _loanDetailsModelBuilder = new ChangeLoanDetailsModelBuilder();
            _loanBuilder = new LoanBuilder(_loanDetailsModelBuilder);
            _lc = new LoanCreator(logRepository.Object, loanHistoryRepository.Object, pacnetService.Object, appCreator.Object, crm.Object, agreementsGenerator.Object, context.Object, _loanBuilder);


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

            pacnetService.Setup(x => x.SendMoney(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(new PacnetReturnData());
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