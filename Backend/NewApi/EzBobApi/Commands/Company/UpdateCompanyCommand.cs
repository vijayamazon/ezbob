namespace EzBobApi.Commands.Company {
    using EzBobApi.Commands.Experian;
    using EzBobCommon.NSB;

    public class UpdateCompanyCommand : CommandBase {
        public CompanyDetailsInfo CompanyDetails { get; set; }
        public ExperianCompanyInfo ExperianCompanyInfo { get; set; }
        public string PromoCode { get; set; }
        public string CustomerId { get; set; }
        public string CompanyId { get; set; }
        public int? Company { get; set; }
        public bool? IsDirectorChecked { get; set; }//TODO: review
        public bool? OwnsProperty { get; set; }
        public string RequestOrigin { get; set; }
    }
}
