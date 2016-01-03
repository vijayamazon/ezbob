using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.HMRC
{
    public class HmrcVatReturnsInfo
    {
        public IEnumerable<VatReturnInfo> VatReturnInfos { get; set; }
        public string TaxOfficeNumber { get; set; }
        public RtiTaxYearInfo RtiTaxYearInfo { get; set; }
    }
}
