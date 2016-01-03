using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.EBay
{
    using EzBobCommon.NSB;

    public class EbayValidationCommandResponse : CommandResponseBase {
        public string Token { get; set; }
        public bool? IsAccountValid { get; set; }
        public IDictionary<string, object> Payload { get; set; } 
    }
}
