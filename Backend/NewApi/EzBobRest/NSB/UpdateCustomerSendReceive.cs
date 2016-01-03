using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobRest.NSB {
    using EzBobApi.Commands.Customer;
    using EzBobCommon.NSB;

    /// <summary>
    /// This class is created automatically by NSB<br/>
    /// imitates synchronous NSB call
    /// </summary>
    public class UpdateCustomerSendReceive : SendRecieveHandler<CustomerUpdateCommandResponse> {}
}
