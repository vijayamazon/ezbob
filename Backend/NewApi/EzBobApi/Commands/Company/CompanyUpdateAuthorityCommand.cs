namespace EzBobApi.Commands.Company {
    using EzBobCommon.NSB;

    public class CompanyUpdateAuthorityCommand : CommandBase {
        public string CustomerId { get; set; }
        public string CompanyId { get; set; }
        public string AuhorityId { get; set; }
        public AuthorityInfo Authority { get; set; }
    }
}
