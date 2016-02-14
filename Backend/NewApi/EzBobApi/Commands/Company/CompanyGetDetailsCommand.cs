namespace EzBobApi.Commands.Company {
    using EzBobCommon.NSB;

    public class CompanyGetDetailsCommand : CommandBase {
        public string CustomerId { get; set; }
        public string CompanyId { get; set; }
    }
}
