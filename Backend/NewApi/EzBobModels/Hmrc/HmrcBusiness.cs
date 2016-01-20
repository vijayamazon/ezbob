using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Hmrc {
    public class HmrcBusiness {
        public int Id { get; set; }
        public string Name { get; set; } //mandatory
        public string Address { get; set; } //mandatory
        public long? RegistrationNo { get; set; }
        public bool? BelongsToCustomer { get; set; }
    }
}
