using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobApi.Commands.Company
{
    using EzBobCommon.NSB;

    public class CompanyUpdateAuthorityCommandResponse : CommandResponseBase
    {
        public string CustomerId { get; set; }
        public string CompanyId { get; set; }
        public string AuthorityId { get; set; }
    }
}
