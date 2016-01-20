using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.Hmrc {
    using EzBobCommon.NSB;

    public class HmrcRegisterCustomerCommand : CommandBase {
        public string CustomerId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
