using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.Ebay
{
    using EzBobCommon.NSB;

    public class EbayRegisterUserCommandResponse : CommandResponseBase
    {
        public bool? IsAccountValid { get; set; }
    }
}
