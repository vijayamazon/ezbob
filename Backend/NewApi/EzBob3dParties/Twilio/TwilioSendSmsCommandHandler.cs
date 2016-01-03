using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Twilio {
    using EzBob3dPartiesApi.Twilio;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels.ThirdParties.Twilio;
    using global::Twilio;
    using NServiceBus;

    /// <summary>
    /// Handles send SMS command
    /// </summary>
    public class TwilioSendSmsCommandHandler : HandlerBase<TwilioSendSmsCommandResponse>, IHandleMessages<TwilioSendSmsCommand> {

        [Injected]
        public ITwilio Twilio { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(TwilioSendSmsCommand command) {
            InfoAccumulator info = new InfoAccumulator();
            TwilioSms sms = null;
            if (string.IsNullOrEmpty(command.Message) || string.IsNullOrEmpty(command.PhoneNumber)) {
                info.AddError("got empty message or phone number");
            } else {
                Message response = await Twilio.SendSms(command.PhoneNumber, command.Message);
                if (response == null) {
                    info.AddError("error in sending sms");
                } else {
                    sms = GetTwilioSms(response);
                }
            }

            SendReply(info, command, resp => resp.Sms = sms);
        }


        /// <summary>
        /// Gets the twilio SMS.
        /// </summary>
        /// <param name="msg">The SMS response.</param>
        /// <returns></returns>
        private TwilioSms GetTwilioSms(Message msg)
        {
            DateTime utcNow = DateTime.UtcNow;
            return new TwilioSms
            {
                AccountSid = msg.AccountSid,
                Sid = msg.Sid,
                DateCreated = msg.DateCreated == default(DateTime) ? utcNow : msg.DateCreated,
                DateSent = msg.DateSent == default(DateTime) ? utcNow : msg.DateSent,
                DateUpdated = msg.DateUpdated == default(DateTime) ? utcNow : msg.DateUpdated,
                From = msg.From,
                To = msg.To,
                Body = msg.Body == null ? "" : (msg.Body.Length > 255 ? msg.Body.Substring(0, 255) : msg.Body),
                Status = msg.Status,
                Direction = msg.Direction,
                Price = msg.Price,
                ApiVersion = msg.ApiVersion,
            };
        }
    }
}
