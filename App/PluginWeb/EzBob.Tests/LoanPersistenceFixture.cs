using System;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using NHibernate;
using NUnit.Framework;
using UnitTests.Utils;

namespace EzBob.Tests
{
    public class LoanPersistenceFixture : InMemoryDbTestFixtureBase
    {
        private ISession _session;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            InitialiseNHibernate(typeof(Customer).Assembly, typeof(User).Assembly);
        }

        [SetUp]
        public void SetUp()
        {
            _session = CreateSession();
        }

        [Test]
        public void saves_schedule_and_transaction_with_loan()
        {
            var type = new StandardLoanType(){Id = 1, Name = "Standard"};
            var loan = new Loan(){Balance = 1000, Date = DateTime.UtcNow, LoanAmount = 1000, Status = LoanStatus.Live, LoanType = type};           

            loan.Schedule.Add(new LoanScheduleItem(){AmountDue = 333, Date = DateTime.UtcNow.AddDays(1), Interest = 10});
            loan.Schedule.Add(new LoanScheduleItem(){AmountDue = 333, Date = DateTime.UtcNow.AddDays(2), Interest = 10});
            loan.Schedule.Add(new LoanScheduleItem(){AmountDue = 333, Date = DateTime.UtcNow.AddDays(3), Interest = 10});
            using(var tx = _session.BeginTransaction())
            {
                _session.Save(type);
                _session.Save(loan);
                tx.Commit();
            }
        }
    }
}