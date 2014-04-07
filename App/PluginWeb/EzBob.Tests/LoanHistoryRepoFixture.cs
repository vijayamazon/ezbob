using System;
using System.Linq;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using NHibernate;
using NUnit.Framework;

namespace EzBob.Tests
{
    public class LoanHistoryRepoFixture : InMemoryDbTestFixtureBase
    {
        private ISession _session;
        private LoanHistoryRepository _lhrepo;
        private Loan _loan;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            InitialiseNHibernate(typeof(Customer).Assembly, typeof(User).Assembly);
        }

        [SetUp]
        public void SetUp()
        {
            _session = CreateSession();
            _lhrepo = new LoanHistoryRepository(_session);

            using (var tx = _session.BeginTransaction())
            {
                var type = new StandardLoanType() { Id = 1, Name = "Standard" };
                _loan = new Loan(){LoanType = type};
                _session.Save(type);
                _session.Save(_loan);
                tx.Commit();
            }            
        }

        [Test]
        public void can_save_loan_history_item()
        {
            var lh = new LoanHistory { Balance = 99.98M, Date = DateTime.Now, Interest = 0.5M, Principal = 99.48M, Loan = _loan };

            _lhrepo.Save(lh);

            var historyItems = _lhrepo.GetByLoan(_loan).ToList();

            Assert.That(historyItems.Count, Is.EqualTo(1));
            Assert.That(historyItems[0].Balance, Is.EqualTo(99.98M));
        }

        [Test]
        public void fetches_most_rescent_history_by_loan()
        {
            var date = DateTime.Now;

            //loan ititial state
            var i1 = new LoanHistory() {Balance = 100, Loan = _loan, Date = date.AddDays(-10)};

            //partial payment fees
            var i2 = new LoanHistory() { Balance = 90, Loan = _loan, Date = date.AddDays(-8).AddHours(2), ExpectedAmountDue = 50, ExpectedFees = 10, ExpectedInterest = 10, ExpectedPrincipal = 30};
            
            //partial payment balance
            var i3 = new LoanHistory() { Balance = 50, Loan = _loan, Date = date.AddDays(-8).AddHours(4), ExpectedAmountDue = 40, ExpectedFees = 0, ExpectedInterest = 10, ExpectedPrincipal = 30};
            
            //early payment
            var i4 = new LoanHistory() { Balance = 0, Loan = _loan, Date = date.AddDays(0).AddHours(2) };

            _lhrepo.Save(i1);
            _lhrepo.Save(i2);
            _lhrepo.Save(i3);
            _lhrepo.Save(i4);

            var h = _lhrepo.FetchHistoryByDay(_loan, date.AddDays(-8));

            Assert.That(h.Before.Balance, Is.EqualTo(100));
            Assert.That(h.Expected.ExpectedAmountDue, Is.EqualTo(50));
            Assert.That(h.After.Balance, Is.EqualTo(50));

        }
    }
}