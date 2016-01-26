using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezbob.Backend.Models.Investor
{
    using System.Runtime.Serialization;

    [DataContract(IsReference = true)]
    public class OneInvestorModel
    {
        [DataMember]
        public int InvestorID { get; set; }
       
        [DataMember]
        public int InvestorTypeID { get; set; }
        
        [DataMember]
        public string InvestorType { get; set; }

        [DataMember]
        public string CompanyName { get; set; }
     
        [DataMember]
        public decimal FundingLimitForNotification { get; set; }

        [DataMember]
        public bool IsActive { get; set; }
    }
}
