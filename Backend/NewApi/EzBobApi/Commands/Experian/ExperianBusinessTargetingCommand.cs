using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.Experian
{
    using EzBobCommon.NSB;

    public class ExperianBusinessTargetingCommand : CommandBase
    {
        public string CompanyName { get; set; }
        public string PostCode { get; set; }
        public bool IsLimited { get; set; }
        public string RegistrationNumber { get; set; }
    }
}
