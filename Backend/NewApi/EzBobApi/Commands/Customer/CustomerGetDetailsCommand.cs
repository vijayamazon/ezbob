using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.Customer
{
    using EzBobCommon.NSB;

    public class CustomerGetDetailsCommand : CommandBase
    {
        public string CustomerId { get; set; }
    }
}
