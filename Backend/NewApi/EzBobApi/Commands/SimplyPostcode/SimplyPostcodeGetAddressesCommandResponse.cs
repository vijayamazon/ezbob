using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.SimplyPostcode
{
    using EzBobCommon.NSB;
    using EzBobModels.SimplyPostcode;

    /// <summary>
    /// Resposne to <see cref="SimplyPostcodeGetAddressesCommand"/>.
    /// </summary>
    public class SimplyPostcodeGetAddressesCommandResponse : CommandResponseBase
    {
        public IList<SimplyPostcodeAddress> Addresses { get; set; }
    }
}
