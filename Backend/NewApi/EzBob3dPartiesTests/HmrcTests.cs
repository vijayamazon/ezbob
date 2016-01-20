namespace EzBob3dPartiesTests {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using EzBob3dParties.HMRC;
    using EzBob3dPartiesTests.Properties;
    using EzBobCommon.Web;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using StructureMap;

    [TestFixture]
    public class HmrcTests : TestBase {
        private static readonly string HmrcGovUk = "https://online.hmrc.gov.uk";

        //the user without VatReturnInfos
        private readonly User user1 = new User {
            UserName = "StartsWith829EndsWith60",
            Password = "NumberMonthYear"
        };

        //the user with VatReturnInfos
        private readonly User user2 = new User {
            UserName = "StartWith362EndsWith27",
            Password = "Some password"
        };

        private class User {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        private string[] returns;

        private int returnIndex;

        [SetUp]
        private void Init() {
            this.returnIndex = -1;
            this.returns = this.GetReturns()
                .ToArray();
        }

        [TearDown]
        private void Finish() {
            this.returnIndex = -1;
        }

        [Test]
        public async void TestHmrc() {

            /* Sequence of requests
             *  "https://online.hmrc.gov.uk"
                "https://online.hmrc.gov.uk/login?GAURI=https%3a%2f%2fonline.hmrc.gov.uk%2fhome"
                "https://online.hmrc.gov.uk/home/services"
                "https://online.hmrc.gov.uk/paye/org/120/FA63318/account"
                "https://online.hmrc.gov.uk/vat-file/trader/117778386/periods"
                "https://online.hmrc.gov.uk/vat-file/trader/119174515/period/496/return"-> N times 
             * 
             */


            IContainer container = InitContainer(typeof(IHmrcService));

            User[] users = {
                this.user1, this.user2
            };

            for (int i = 0; i < users.Length; ++i) {
                var browserStub = CreateWebBrowserStub(i + 1);

                container.Configure(c => c.For<IEzBobWebBrowser>()
                    .Use(browserStub.Object));

                var hmrc = container.GetInstance<IHmrcService>();
                HmrcVatReturnsInfo hmrcVatReturnsInfo = await hmrc.GetVatReturns(users[i].UserName, users[i].Password);

                if (hmrcVatReturnsInfo.VatReturnInfos != null) {
                    foreach (var info in hmrcVatReturnsInfo.VatReturnInfos) {
                        //for test we do not compare pdfs
                        info.PdfFile = null;
                    }
                }

                Assert.True(CompareWithExpected(hmrcVatReturnsInfo, i + 1), "got unexpected json");
            }
        }

        private bool CompareWithExpected(HmrcVatReturnsInfo result, int userId) {
            string expectedJson;
            if (userId == 1) {
                expectedJson = Resources.Result1;
            } else {
                expectedJson = Resources.Result2;
            }

            if (result.VatReturnInfos != null) {
                var expected = JsonConvert.DeserializeObject<HmrcVatReturnsInfo>(expectedJson);
                expected.VatReturnInfos = expected.VatReturnInfos.OrderBy(o => o.FromDate)
                    .ToArray();

                expectedJson = JsonConvert.SerializeObject(expected);

                result.VatReturnInfos = result.VatReturnInfos.OrderBy(o => o.FromDate)
                    .ToArray();
            }


            string resultJson = JsonConvert.SerializeObject(result);

            var resultToken = JObject.Parse(resultJson);
            var expectedToken = JObject.Parse(expectedJson);

            return JToken.DeepEquals(resultToken, expectedToken);
        }

        private Mock<IEzBobWebBrowser> CreateWebBrowserStub(int userNumber) {
            var httpClientStub = new Mock<IEzBobWebBrowser>();

            httpClientStub.Setup(o => o.DownloadPageAsyncAsString(It.Is<string>(s => !string.IsNullOrEmpty(s) && s.Contains("https://online.hmrc.gov.uk"))))
                .Returns(() => CreateCompletedTask(Resources.HmrcLoginHtml));

            httpClientStub.Setup(o => o.PostAsyncAndGetStringResponse(It.Is<string>(s => !string.IsNullOrEmpty(s) && s.Contains("GAURI")), It.Is<HttpContent>(h => h != null)))
                .Returns(() => CreateCompletedTask(this.SelectLoginResponse(userNumber)));

            httpClientStub.Setup(o => o.DownloadPageAsyncAsString(It.Is<string>(s => !string.IsNullOrEmpty(s) && s.Contains("home") && s.Contains("services"))))
                .Returns(() => CreateCompletedTask(SelectHomeServices(userNumber)));

            httpClientStub.Setup(o => o.DownloadPageAsyncAsString(It.Is<string>(s => !string.IsNullOrEmpty(s) && s.Contains("/paye/org/") && s.Contains("/account"))))
                .Returns(() => CreateCompletedTask(SelectAccountPayerOrg(userNumber)));

            httpClientStub.Setup(o => o.DownloadPageAsyncAsString(It.Is<string>(s => !string.IsNullOrEmpty(s) && s.Contains("trader") && s.Contains("periods"))))
                .Returns(() => CreateCompletedTask(SelectVatFileTraderPeriods(userNumber)));

            httpClientStub.Setup(o => o.DownloadPageAsyncAsString(It.Is<string>(s => !string.IsNullOrEmpty(s) && s.Contains("period") && s.Contains("return"))))
                .Returns(GetNextCompletedReturnTask);

            return httpClientStub;
        }

        private string SelectLoginResponse(int userId) {
            if (userId == 1) {
                return Resources.HmrcLoginResponse1;
            }

            return Resources.HmrcLoginResponse2;
        }

        private string SelectHomeServices(int userId) {
            if (userId == 1) {
                return Resources.HomeServices1;
            }
            return Resources.HomeServices2;
        }

        private string SelectAccountPayerOrg(int userId) {
            if (userId == 1) {
                return Resources.AccountPayerOrg1;
            }

            return Resources.AccountPayerOrg2;
        }

        private string SelectVatFileTraderPeriods(int userId) {
            if (userId == 1) {
                return Resources.VatFileTraderPeriods1;
            }

            return Resources.VatFileTraderPeriods2;
        }

        private Task<string> GetNextCompletedReturnTask() {
            int idx = Interlocked.Increment(ref this.returnIndex);
            return CreateCompletedTask(this.returns[idx]);
        }

        private IEnumerable<string> GetReturns() {
            yield return Resources.Return1;
            yield return Resources.Return2;
            yield return Resources.Return3;
            yield return Resources.Return4;
            yield return Resources.Return5;
        }
    }
}
