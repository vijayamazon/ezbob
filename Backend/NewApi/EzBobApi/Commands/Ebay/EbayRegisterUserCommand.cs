using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.Ebay
{
    using EzBobCommon.NSB;

    public class EbayRegisterUserCommand : CommandBase
    {
        public int CustomerId { get; set; }
        public string SessionId { get; set; }
        public string Token { get; set; }
        public string MarketplaceName { get; set; }
    }
}
