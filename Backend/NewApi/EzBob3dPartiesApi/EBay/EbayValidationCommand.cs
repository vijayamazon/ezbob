using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.EBay
{
    using EzBobCommon.NSB;

    public class EbayValidationCommand : CommandBase
    {
        public string Token { get; set; }

        public bool? IsValidateUserAccount { get; set; }

        public IDictionary<string, object> PayLoad { get; set; } 
    }
}
