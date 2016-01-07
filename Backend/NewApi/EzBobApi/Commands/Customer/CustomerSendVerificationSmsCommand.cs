using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.Customer
{
    using EzBobCommon.NSB;

    public class CustomerSendVerificationSmsCommand : CommandBase
    {
        public string CustomerId { get; set; }
        public string PhoneNumber { get; set; }
        public string MessageHeader { get; set; }
        public string MessageFooter { get; set; }
    }
}
