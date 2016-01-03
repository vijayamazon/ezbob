using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.Twilio
{
    using EzBobCommon.NSB;
    using EzBobModels.ThirdParties.Twilio;

    public class TwilioSendSmsCommandResponse : CommandResponseBase
    {
        public TwilioSms Sms { get; set; }
    }
}
