using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.Experian
{
    using EzBobCommon.NSB;

    public class ExperianBusinessTargetingCommandResponse : CommandResponseBase
    {
        public ExperianCompanyInfo[] CompanyInfos { get; set; }
    }
}
