namespace EzBobApi.Commands.Company
{
    using System.Collections.Generic;
    using EzBobCommon.NSB;

    /// <summary>
    /// Response to <see cref="CompanyGetDetailsCommand"/>
    /// </summary>
    public class CompanyGetDetailsCommandResponse : CommandResponseBase
    {
        public string CustomerId { get; set; }
        public string CompanyId { get; set; }
        public CompanyDetailsInfo CompanyDetails { get; set; }
        public IEnumerable<AuthorityInfo> Authorities { get; set; }
    }
}
