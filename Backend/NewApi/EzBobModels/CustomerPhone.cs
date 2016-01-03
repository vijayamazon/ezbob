using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels
{
    public class CustomerPhone
    {
        public int? Id { get; set; }
        public int CustomerId { get; set; }
        public string PhoneType { get; set; }
        public string Phone { get; set; }
        public bool? IsVerified { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string VerifiedBy { get; set; }
        public bool IsCurrent { get; set; }
    }
}
