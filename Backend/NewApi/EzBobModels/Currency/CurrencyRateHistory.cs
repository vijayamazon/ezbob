using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Currency
{
    public class CurrencyRateHistory
    {
        public int Id { get; set; }
        public int CurrencyDataId { get; set; }
        public decimal? Price { get; set; }
        public DateTime? Updated { get; set; }
    }
}
