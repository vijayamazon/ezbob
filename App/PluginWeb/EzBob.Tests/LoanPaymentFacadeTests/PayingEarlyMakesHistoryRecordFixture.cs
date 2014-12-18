using System;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using Moq;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    public class PayingEarlyMakesHistoryRecordFixture : InMemoryDbTestFixtureBase
    {
        private Loan _loan;
        private DateTime _startDate;
        private decimal _takenMoney = 3000;
        private LoanPaymentFacade _facade;
        private Customer _customer;
        private Mock<ILoanHistoryRepository> _historyRepoMock;
	    private Mock<LoanTransactionMethodRepository> _loanTransatioMethodReporsitory;

        [SetUp]
        public void SetUp()
        {
            _historyRepoMock = new Mock<ILoanHistoryRepository>();

            _loan = new Loan() { Id = 1, Status = LoanStatus.Live };
            var calculator = new LoanScheduleCalculator();
            _startDate = new DateTime(2012, 1, 1);
            calculator.Calculate(_takenMoney, _loan, _startDate);
			_facade = new LoanPaymentFacade(_historyRepoMock.Object, _loanTransatioMethodReporsitory.Object);
            _customer = new Customer();
            _customer.Loans.Add(_loan);
            _loan.Customer = _customer;
        }

        [Test]
        public void paying_early_save_new_loan_to_history()
        {
            LoanHistory copy = null;

            _historyRepoMock.Setup(x => x.Save(It.IsAny<LoanHistory>())).Callback<LoanHistory>(lh =>
                {
                    copy = lh;
                });

            _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate);

            _historyRepoMock.Verify(x => x.Save(It.IsAny<LoanHistory>()), Times.Exactly(1));

            Assert.That(copy.Balance, Is.EqualTo(_loan.Balance));
            Assert.That(copy.Interest, Is.EqualTo(_loan.Interest));
            Assert.That(copy.Principal, Is.EqualTo(_loan.Principal));
        }

    }
}