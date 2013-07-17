using System;
using EzBob.CommonLib;
using EzBob.TeraPeakServiceLib;
using EzBob.TeraPeakServiceLib.Stub;
using NUnit.Framework;

namespace EzBob.Tests.Terapeak
{
    [TestFixture]
    public class CategoriesFixture
    {
        private ITeraPeakCredentionProvider _credetnialsProvider;
        private ITeraPeakConnectionProvider _connectionProvider;

        [SetUp]
        public void SetUp()
        {
            //_credetnialsProvider = new TeraPeakCredentionProviderProduction();
            _credetnialsProvider = new CredentialsProviderStub();
            _connectionProvider = new ConnectionProviderStub();
        }

        [Test]
        [Ignore]
        public void request_data()
        {
            //var requestInfo = CreateRequestInfo("headboardsdirect");
            var requestInfo = TerapeakRequestInfoBuilder.CreateRequestInfo("megastar4", 10);
            var data = TeraPeakService.SearchBySeller(_connectionProvider, _credetnialsProvider, requestInfo);
            Assert.That(data.Count, Is.GreaterThan(0));
        }
    }
}