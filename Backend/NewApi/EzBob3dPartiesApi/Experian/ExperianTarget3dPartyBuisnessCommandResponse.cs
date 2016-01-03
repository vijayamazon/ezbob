using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.Experian
{
    using EzBobCommon.NSB;
    using EzBobModels.ThirdParties.Experian;

    public class ExperianTarget3dPartyBuisnessCommandResponse : CommandResponseBase
    {
        public Experian3dPartyCompanyInfo[] CompanyInfos { get; set; }
    }
}
