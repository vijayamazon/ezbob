using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Yodlee
{
    public class YodleeUserAccount
    {
        public virtual int Id { get; set; }
        public virtual int? CustomerId { get; set; }
        public virtual int? BankId { get; set; }
        public virtual string Username { get; set; }
        public virtual string Password { get; set; }
        public virtual DateTime? CreationDate { get; set; }
    }
}
