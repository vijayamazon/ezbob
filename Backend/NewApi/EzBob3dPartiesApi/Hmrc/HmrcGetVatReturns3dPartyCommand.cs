namespace EzBob3dPartiesApi.Hmrc {
    using EzBobCommon.NSB;

    public class HmrcGetVatReturns3dPartyCommand : CommandBase {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string CustomerId { get; set; }
    }
}
