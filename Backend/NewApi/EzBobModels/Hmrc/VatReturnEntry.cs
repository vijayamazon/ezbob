using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Hmrc
{
    public class VatReturnEntry
    {
        public int Id { get; set; }
        public int RecordId { get; set; }
        public int NameId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
