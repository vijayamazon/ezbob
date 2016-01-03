using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Twilio
{
    using EzBobCommon;
    using EzBobModels.ThirdParties.Twilio;
    using global::Twilio;
    using log4net;

    /// <summary>
    /// Uses Twilo rest client to send sms
    /// </summary>
    internal class TwilioService : ITwilio {
        [Injected]
        public TwilioConfig Config { get; set; }

        [Injected]
        public ILog Log { get; set; }

        /// <summary>
        /// Sends the SMS.
        /// </summary>
        /// <param name="toPhoneNumber">To phone number.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public Task<Message> SendSms(string toPhoneNumber, string message) {
            return Task.Run(() => {
                Message smsResponse = GetTwilioRestClient()
                    .SendMessage(Config.TwilioSendingNumber, toPhoneNumber, message);

                if (smsResponse.RestException != null) {
                    string errorMsg = string.Format("RestException Code:{0}, Status:{3}, Message:{1}, MoreInfo:{2}",
                        smsResponse.RestException.Code, smsResponse.RestException.Message, smsResponse.RestException.MoreInfo, smsResponse.RestException.Status);
                    Log.Error("could not send sms to " + toPhoneNumber + " ; " + errorMsg);
                    return null;
                }

                return smsResponse;
            });
        }

        private TwilioRestClient GetTwilioRestClient() {
            return new TwilioRestClient(Config.TwilioAccountSid, Config.TwilioAuthToken);
        }
    }
}
