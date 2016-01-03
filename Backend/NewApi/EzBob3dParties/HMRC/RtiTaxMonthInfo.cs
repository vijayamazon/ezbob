using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.HMRC
{
    using EzBobCommon.Currencies;

    public class RtiTaxMonthInfo
    {
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public Money AmountPaid { get; set; }
        public Money AmountDue { get; set; }
    }
}
