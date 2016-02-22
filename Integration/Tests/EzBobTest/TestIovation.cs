namespace EzBobTest {
    using System.Linq;
    using Ezbob.Backend.Models;
    using Ezbob.Backend.Strategies;
    using Ezbob.Backend.Strategies.Iovation;
    using IovationLib;
    using log4net;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class TestIovation : BaseTestFixtue {
        protected readonly ILog Log = LogManager.GetLogger(typeof(TestIovation));

        [SetUp]
        public void Init() {
            log4net.Config.XmlConfigurator.Configure();
            Library.Initialize(this.m_oEnv, this.m_oDB, this.m_oLog);
        }

        [Test]
        public void TestIovationCheck() {

            var request = new IovationCheckModel {
                CustomerID = 1350,
                AccountCode = "01040628",
                BeginBlackBox = "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",
                Email = "stasd+io10@ezbob.com",
                EndUserIp = "::1",
                MobilePhoneNumber = "01111111111",
                Origin = "signup",
                Type = "application",
                mobilePhoneVerified = true,
                mobilePhoneSmsEnabled = true

            };

            var response = GetTestClient().CheckTransactionDetails(request);

            this.Log.Info(JsonConvert.SerializeObject(response, Formatting.Indented));

            var score = response.details.FirstOrDefault(x => x.name == "ruleset.score");
            if(score!= null)
            this.Log.InfoFormat("score {0}", score.value);

        }

        [Test]
        public void TestIovationStrategy() {
            var request = new IovationCheckModel {
                CustomerID = 1350,
                AccountCode = "01040628",
                BeginBlackBox = "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",
                Email = "stasd+io10@ezbob.com",
                EndUserIp = "::1",
                MobilePhoneNumber = "01111111111",
                Origin = "signup",
                Type = "application",
                mobilePhoneVerified = true,
                mobilePhoneSmsEnabled = true

            };
            IovationCheck check = new IovationCheck(request);
            check.Execute();
        }

        private IovationAppClient GetTestClient() {
            IovationAppClient clientTest = new IovationAppClient("962002", "OLTP", "5BM9NDY2", "345yP0und5", "orange", "Test");
            return clientTest;
        }

        private IovationAppClient GetProdClient() {
            IovationAppClient clientProd = new IovationAppClient("962002", "OLTP", "5BM9NDY2", "345yP0und5", "orange", "Production");
            return clientProd;
        }
    }
}
