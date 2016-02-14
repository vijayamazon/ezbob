namespace EzBobRestTests {
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using EzBobApi.Commands.Customer;
    using EzBobApi.Commands.Customer.Sections;
    using EzBobCommon;
    using EzBobRest;
    using Moq;
    using Newtonsoft.Json;
    using NServiceBus;
    using NUnit.Framework;
    using StructureMap;
    using Thinktecture.IdentityModel.Client;

    [TestFixture]
    public class RestServerTests : RestServerTestBase {

        [Test]
        public void Obtain_OAuth_token_and_use_it_to_get_some_response() {
            StartTest(RegisterBusMock, restServer =>
            {
                var token = GetBearerToken(restServer.Config.ServerAddress)
                    .Result;
                Assert.True(token != null && token.AccessToken != null, "could not get token");
                string response = DoRestCall(token.AccessToken, restServer.Config.ServerAddress + "/api/v1");
                Assert.False(string.IsNullOrEmpty(response), "got empty response");
            });
        }

        [Test]
        [Ignore("Need to update to reflect recent changes")]
        public void TestSignup() {
            StartTest(RegisterBusMock, restServer => {
                using (var httpClient = GetHttpClient(restServer)) {

                    CustomerSignupCommand command = new CustomerSignupCommand {
//                        Account = new AccountInfo {
//                            EmailAddress = "blabla"
//                        }
                    };

                    using (var content = httpClient
                        .PostAsync("api/v1/customer/signup", CreateHttpContent(command))
                        .Result.Content) {

                        string res = content.ReadAsStringAsync()
                            .Result;

                        var info = JsonConvert.DeserializeObject<InfoAccumulator>(res);
                        Assert.True(info.HasErrors && info.GetErrors()
                            .FirstOrDefault(er => er.Contains("pass")) != null, "expected to get error");

                        int kk = 0;
                    }
                }
            });
        }

        [Test]
        public void ManualTest() {

            string json = "{'AddressInfo':{'Organization':'org','Line1':'l1','Town':'Ttt'},'ContactDetailsInfo':{'EmailAddress':'lala'},'PersonalDetailsInfo':{'FirstName':'string','MiddleName':'string','SurName':'string','Gender':'string','MeritalStatus':0},'OwnedPropertyAddressInfo':[{'IsLivingNow':true,'MonthsAtAddress':'22'}],'LivingAccommodationInfo':[{'HousingType':'4'}]}";
            json = "{'AddressInfo':{'Organization':'org','Line1':'l1','Town':'Ttt'}}";

            var res = JsonConvert.DeserializeObject<CustomerUpdateCommand>(json);

            StartTest(RegisterBusMock, restServer => {
                Thread.Sleep(TimeSpan.FromHours(2));
            });
        }

        private void RegisterBusMock(IContainer container) {
            var busMock = new Mock<IBus>();
            container.Configure(c => c.ForSingletonOf<IBus>()
                .Use(busMock.Object));
        }

        /// <summary>
        /// Creates the content of the HTTP.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private StringContent CreateHttpContent(object obj) {
            JsonSerializerSettings settings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(obj, settings);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// Gets the HTTP client.
        /// </summary>
        /// <param name="restServer">The rest server.</param>
        /// <returns></returns>
        private HttpClient GetHttpClient(RestServer restServer) {
            var token = GetBearerToken(restServer.Config.ServerAddress)
                .Result;
            var client = new HttpClient();
            client.BaseAddress = new Uri(restServer.Config.ServerAddress);
            client.SetBearerToken(token.AccessToken);
            return client;
        }

        /// <summary>
        /// Gets the bearer token.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private Task<TokenResponse> GetBearerToken(string url) {
            url = url + "/token";
            var client = new OAuth2Client(new Uri(url));
            return client.RequestResourceOwnerPasswordAsync("test", "test");
        }

        /// <summary>
        /// Does the rest call.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private string DoRestCall(string token, string url) {
            using (var client = new HttpClient()) {
                client.SetBearerToken(token);
                var response = client.GetStringAsync(new Uri(url)).Result;
                return response;
            }
        }
    }
}
