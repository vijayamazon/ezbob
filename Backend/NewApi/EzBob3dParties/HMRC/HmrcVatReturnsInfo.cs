namespace EzBob3dParties.Hmrc
{
    using System.Collections.Generic;
    using EzBobCommon;

    public class HmrcVatReturnsInfo
    {
        public IEnumerable<VatReturnInfo> VatReturnInfos { get; set; }
        public string TaxOfficeNumber { get; set; }
        public RtiTaxYearInfo RtiTaxYearInfo { get; set; }
        public InfoAccumulator Info { get; set; }
    }
}
