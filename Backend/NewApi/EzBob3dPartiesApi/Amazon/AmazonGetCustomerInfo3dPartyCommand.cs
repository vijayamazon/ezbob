using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.Amazon
{
    using EzBobCommon.NSB;

    public class AmazonGetCustomerInfo3dPartyCommand : CommandBase
    {
        public string SellerId { get; set; }
    }
}
