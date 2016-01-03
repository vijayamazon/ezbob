using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.Experian
{
    using EzBobCommon;
    using EzBobCommon.NSB;

    public class ExperianTarget3dPartyBusinessCommand : CommandBase
    {
        public string CompanyName { get; set; }

        public string PostCode { get; set; }
        public bool IsLimited { get; set; }
        public Optional<string> RegNumber { get; set; }
    }
}
