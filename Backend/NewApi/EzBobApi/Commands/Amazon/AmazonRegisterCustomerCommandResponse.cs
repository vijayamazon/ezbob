using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.Amazon
{
    using EzBobCommon.NSB;

    public class AmazonRegisterCustomerCommandResponse : CommandResponseBase
    {
        public string CustomerId { get; set; }
    }
}
