using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.Yodlee
{
    using EzBobCommon.NSB;

    public class YodleeUserAddedAccountCommand : CommandBase
    {
        public int CustomerId { get; set; }
    }
}
