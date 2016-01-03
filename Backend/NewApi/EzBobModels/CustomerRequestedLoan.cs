using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels
{
    public class CustomerRequestedLoan {
        public int? Id { get; set; }
        public int CustomerId { get; set; }
        public int? ReasonId { get; set; }
        public int? SourceOfRepaymentId { get; set; }
        public DateTime Created { get; set; }
        public double? Amount { get; set; }
        public string OtherReason { get; set; }
        public string OtherSourceOfRepayment { get; set; }
    }
}
