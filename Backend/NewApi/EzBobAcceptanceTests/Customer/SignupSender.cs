using System;

namespace EzBobAcceptanceTests.Customer
{
    using System.Net.Http;
    using EzBobAcceptanceTests.Customer.Requests;
    using EzBobAcceptanceTests.Customer.Responses;
    using EzBobAcceptanceTests.Infra;
    using EzBobCommon.Http;
    using Newtonsoft.Json;

    public static class SignupSender
    {
        public static void CustomerSignupSendRecieve(Action<SignupRequest> beforeSend, Action<CustomerSignupResponse> onResponse)
        {
            SignupRequest request = new SignupRequest();
            beforeSend(request);

            RestClient restClient = new RestClient();
            var httpClient = restClient.GetClient();

            HttpResponseMessage responseMessage = httpClient.PostAsync("http://localhost:12345/api/v1/customer/signup", new JsonContent(request.ToString())).Result;
            string responseJson = responseMessage.Content.ReadAsStringAsync()
                .Result;

            var customerSignupResponse = JsonConvert.DeserializeObject<CustomerSignupResponse>(responseJson);

            if (onResponse != null) {
                onResponse(customerSignupResponse);
            }
        }
    }
}
