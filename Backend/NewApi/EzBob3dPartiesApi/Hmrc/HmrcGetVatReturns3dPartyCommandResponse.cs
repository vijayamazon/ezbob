namespace EzBob3dPartiesApi.Hmrc {
    using System.Collections.Generic;
    using EzBobCommon.NSB;
    using EzBobModels.Hmrc;

    public class HmrcGetVatReturns3dPartyCommandResponse : CommandResponseBase {
        public IEnumerable<VatReturnsPerBusiness> VatReturnsPerBusiness { get; set; }
        public IEnumerable<RtiTaxMonthEntry> RtiMonthEntries { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string CustomerId { get; set; }
    }
}
