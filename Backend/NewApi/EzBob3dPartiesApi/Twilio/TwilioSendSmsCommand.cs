using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.Twilio
{
    using EzBobCommon.NSB;

    public class TwilioSendSmsCommand : CommandBase
    {
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
    }
}
