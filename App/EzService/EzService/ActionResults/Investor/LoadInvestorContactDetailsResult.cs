
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.ActionResults.Investor
{
    using System.Runtime.Serialization;
    using Ezbob.Backend.Models.Investor;

    [DataContract]
    public class LoadInvestorContactDetailsResult : ActionResult
    {
        [DataMember]
        public List<InvestorContactModel> InvestorContactDetails { get; set; }
    }
}

