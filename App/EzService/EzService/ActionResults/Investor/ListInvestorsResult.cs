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
    public class ListInvestorsResult : ActionResult
    {
        [DataMember]
        public List<OneInvestorModel> Investors { get; set; }
    }
}

