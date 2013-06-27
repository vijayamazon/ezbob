using System;
using EzBob.CommonLib;
using EzBob.TeraPeakServiceLib;
using EzBob.TeraPeakServiceLib.Requests.ResearchResult;
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
            //var requestInfo = CreateRequestInfoe("headboardsdirect");
            var requestInfo = CreateRequestInfoe("megastar4");
            var data = TeraPeakService.SearchBySeller(_connectionProvider, _credetnialsProvider, requestInfo);
            Assert.That(data.Count, Is.GreaterThan(0));
        }

        private static TeraPeakRequestInfo CreateRequestInfoe(string ebayUserID)
        {
            var sellerInfo = new TeraPeakSellerInfo(ebayUserID);
            var countMonths = 10;
            var now = DateTime.UtcNow;
            var startDate = now.Date.AddYears(-1).AddDays(-1);

            var peakRequestDataInfo = new TeraPeakRequestDataInfo
            {
                StepType = TeraPeakRequestStepEnum.ByMonth,
                CountSteps = countMonths,
                StartDate = startDate,
            };
            var ranges = TerapeakRequestsQueue.CreateQueriesDates(peakRequestDataInfo, now);
            var retryInfo = new ErrorRetryingInfo() {EnableRetrying = false};

            var requestInfo = new TeraPeakRequestInfo(sellerInfo, ranges, retryInfo);
            return requestInfo;
        }

        private class CredentialsProviderStub : ITeraPeakCredentionProvider
        {
            public TeraPeakRequesterCredentials RequesterCredentials
            {
                get
                {
                    return IsNewVersionOfCredentials ? null : new TeraPeakRequesterCredentials
                        {
                            Token = "bf69db0c2ad55c207ec9c01f793cf0",
                            UserToken = "5d4f3089986df6cced1e367dda87ee",
                            DeveloperName = "alex_syrotyuk_alex_syrotyuk"
                        };
                }
            }

            public string ApiKey
            {
                get { return "xdz8d8hw4cp5x9napc4s7tpq"; }
            }

            public bool IsNewVersionOfCredentials
            {
                get { return !string.IsNullOrEmpty(ApiKey); }
            }
        }

        private class ConnectionProviderStub : ITeraPeakConnectionProvider
        {
            public string Url
            {
                get { return "http://api.terapeak.com/v1/research/xml/restricted"; }
            }
        }
    }
}