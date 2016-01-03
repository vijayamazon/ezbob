using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.PAYPal {
    using System.Globalization;
    using EzBobCommon;
    using PayPal.Api;
    using PayPal.Api.OpenIdConnect;

    /// <summary>
    /// Uses PayPal REST API  
    /// </summary>
    public class PayPalService {
        [Injected]
        public PayPalConfig Config { get; set; }


        /// <summary>
        /// Gets the user information.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        public Task<Userinfo> GetUserInfo(string accessToken)
        {
            var userInfoParams = new UserinfoParameters();
            userInfoParams.SetAccessToken(accessToken);
            return Task.Run(() => Userinfo.GetUserinfo(GetApiContext(), new UserinfoParameters()));
        }

        /// <summary>
        /// Gets the transactions.
        /// </summary>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        public void GetTransactions(DateTime fromDate, DateTime toDate, string payerId) {

            var dateTimeFrom = ConvertDateTimeToDateAndTime(fromDate);
            string startDate = dateTimeFrom.Item1;
            string startTime = dateTimeFrom.Item2;
            var dateTimeTo = ConvertDateTimeToDateAndTime(toDate);
            string endDate = dateTimeTo.Item1;
            string endTime = dateTimeTo.Item2;

            int? count = null;
            string startId = string.Empty;
            int? startIndex = null;

            PaymentHistory history = PayPal.Api.Payment.List(GetApiContext(), count, startId, startIndex, startTime, endTime, startDate, endDate, payerId);

            history.payments.SelectMany(p => p.transactions).GroupBy(t => t.payee.

        }

        private Tuple<string, string> ConvertDateTimeToDateAndTime(DateTime dateTime) {
            string date =  dateTime.ToUniversalTime()
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the login URL to send user to in order to get his consent to share private information with us.
        /// </summary>
        /// <param name="redirectUrl">The redirect URL where PayPal should post authorization token</param>
        /// <returns>url with all parameters required to obtain consent</returns>
        public string GetLoginUrl(string redirectUrl) {
            StringBuilder url = new StringBuilder(Config.AuthorizationUrl);
            url.Append("?client_id=")
                .Append(Config.ClientId);
            url.Append("&amp;response_type=code");
            url.Append("&amp;scope=openid+profile+email+address+phone+https%3A%2F%2Furi.paypal.com%2Fservices%2Fpaypalattributes");
            url.Append("&amp;redirect_uri=")
                .Append(redirectUrl);

            return url.ToString();
        }

        /// <summary>
        /// Gets the API context.
        /// </summary>
        /// <returns></returns>
        private APIContext GetApiContext() {
            var accessToken = new OAuthTokenCredential(Config.ToDictionary()).GetAccessToken();
            return new APIContext(accessToken);
        }
    }
}
