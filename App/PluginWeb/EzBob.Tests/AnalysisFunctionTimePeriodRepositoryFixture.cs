using System;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using NHibernate;
using NUnit.Framework;
using UnitTests.Utils;

namespace EzBob.Tests
{
    [TestFixture]
    public class AnalysisFunctionTimePeriodRepositoryFixture : InMemoryDbTestFixtureBase
    {
        private ISession _session;
        private AnalysisFunctionTimePeriodRepository _AnalysisFunctionTimePeriodRepository;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            InitialiseNHibernate(typeof(MP_AnalysisFunctionTimePeriod).Assembly, typeof(User).Assembly);
        }

        [SetUp]
        public void SetUp()
        {
            _session = CreateSession();
            _AnalysisFunctionTimePeriodRepository = new AnalysisFunctionTimePeriodRepository(_session);
        }

        [Test]
        public void get_returns_timeperiod_by_internal_id()
        {
            var internalId = Guid.Parse("318795D7-C51D-4B18-8E1F-5A563B3091F4");
            var timeperiod = new MP_AnalysisFunctionTimePeriod(){InternalId = internalId,Name = "30", Description = "30 days - 1 month"};
            _AnalysisFunctionTimePeriodRepository.Save(timeperiod);

            var actual = _AnalysisFunctionTimePeriodRepository.Get(internalId);

            Assert.That(actual.Id, Is.Not.EqualTo(0));
            Assert.That(actual.Id, Is.EqualTo(timeperiod.Id));
        }

        [Test]
        public void get_returns_null_if_no_such_timeperiod()
        {
            var internalId = Guid.Parse("318795D7-C51D-4B18-8E1F-5A563B3091F4");

            var actual = _AnalysisFunctionTimePeriodRepository.Get(internalId);

            Assert.That(actual, Is.Null);
        }
    }
}