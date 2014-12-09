namespace EzBob.Tests
{
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Loans;
	using Web.Code;
	using NHibernate;
	using NUnit.Framework;
	using EZBob.DatabaseLib.Model.Database.UserManagement;

	[TestFixture]
    public class ExpectationReportFixture : InMemoryDbTestFixtureBase
    {
        private ISession _session;

        private ILoanScheduleRepository _loanScheduleRepository;
        private ILoanTransactionRepository _transactions;
        private ILoanHistoryRepository _historyRepository;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            InitialiseNHibernate(typeof(Customer).Assembly, typeof(User).Assembly);
        }

        [SetUp]
        public void SetUp()
        {
            _session = CreateSession();
            _loanScheduleRepository = new LoanScheduleRepository(_session);
            _transactions = new LoanTransactionRepository(_session);
            _historyRepository = new LoanHistoryRepository(_session);
        }

        [Test]
        [Ignore]
        public void simple_report()
        {
            var date = new DateTime(2012, 10, 10);
            var loan1 = new Loan() {Date = date.AddDays(-10), Balance = 100, Interest = 10};
            loan1.Customer = new Customer(){PersonalInfo = new PersonalInfo(){Fullname = "Test Test Test"}};

            var installment1 = new LoanScheduleItem() {Loan = loan1, Date = date.AddDays(-9), AmountDue = 50, Balance = 50, Interest = 1};

            var lh1 = new LoanHistory(loan1, date.AddDays(-10)) { Loan = loan1 };
            var lh2 = new LoanHistory(loan1, date.AddDays(-9)) { Loan = loan1, Balance = 51};
            var tran1 = new PaypointTransaction() { Amount = 50, Interest = 1, Balance = 49, Loan = loan1, Fees = 0, PostDate = date.AddDays(-9) };

            using (var tx = _session.BeginTransaction())
            {
                _session.Save(loan1);
                _session.Save(installment1);
                _session.Save(lh1);
                _session.Save(lh2);
                _session.Save(tran1);
                tx.Commit();
            }

            var builder = new DailyReportBuilder(_loanScheduleRepository, _transactions, _historyRepository);
            var report = builder.GenerateReport(2012, 10, 1);
            Assert.That(report.Count, Is.EqualTo(1));
            Assert.That(report[0].Before.Balance, Is.EqualTo(100));
            Assert.That(report[0].After.Balance, Is.EqualTo(51));
            Assert.That(report[0].Paid.Balance, Is.EqualTo(49));
        }
    }
}
