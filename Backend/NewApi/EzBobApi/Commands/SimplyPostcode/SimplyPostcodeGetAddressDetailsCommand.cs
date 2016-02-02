using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.SimplyPostcode
{
    using EzBobCommon.NSB;

    public class SimplyPostcodeGetAddressDetailsCommand : CommandBase
    {
        public string CustomerId { get; set; }
        public string AddressId { get; set; }
    }
}
