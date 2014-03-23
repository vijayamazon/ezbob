using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.Code;
using NHibernate;
using NUnit.Framework;
using UnitTests.Utils;

namespace EzBob.Tests
{
    [TestFixture]
    public class RefNumberFixture : InMemoryDbTestFixtureBase
    {
        private ISession _session;
        private CustomerRepository _customers;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            InitialiseNHibernate(typeof(Customer).Assembly, typeof(User).Assembly);
        }

        [SetUp]
        public void SetUp()
        {
            _session = CreateSession();
            _customers = new CustomerRepository(_session);
        }

        [Test]
        public void can_generate_ref_number()
        {
            var g = new RefNumberGenerator(_customers, 1);
            var r = int.Parse(g.GenerateForCustomer().Replace("01", ""));
            Assert.That(r, Is.LessThan(10));
            Assert.That(r, Is.GreaterThan(0));
        }

        [Test]
        public void can_pad_with_zero()
        {
            var g = new FakeRefNumberGenerator(_customers, 4);
            g.FakeNumber = 1;
            var s = g.GenerateForCustomer();
            Assert.That(s, Is.EqualTo("010001"));
        }

        [Test]
        public void can_solve_conflicts()
        {
            var u1 = new Customer() {Name = "UserOne", RefNumber = "010001"};
            _customers.Save(u1);

            var g = new FakeRefNumberGenerator(_customers, 4);
            g.FakeNumber = 1;
            var s = g.GenerateForCustomer();
            Assert.That(s, Is.EqualTo("010002"));
        }
    }

    internal class FakeRefNumberGenerator : RefNumberGenerator
    {
        public FakeRefNumberGenerator(ICustomerRepository customers) : base(customers)
        {
        }

        public FakeRefNumberGenerator(ICustomerRepository customers, int maxDigits) : base(customers, maxDigits)
        {
        }

        protected override int GenerateRandomNumber(int digits)
        {
            return FakeNumber;
        }

        public int FakeNumber { get; set; }
    }
}