using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels
{
    public class CustomerSession
    {
        public int? Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime StartSession { get; set; }
        public string Ip { get; set; }
        public bool IsPasswdOk { get; set; }
        public string ErrorMessage { get; set; }
    }
}
