using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dPartiesApi.PayPal.Soap
{
    using EzBobCommon.Currencies;

    public class PayPal3dPartyTransactionItem
    {
        public int TransactionId { get; set; }
        public DateTime? Created { get; set; }
        public int? CurrencyId { get; set; }
        public Money FeeAmount { get; set; }
        public Money GrossAmount { get; set; }
        public Money NetAmount { get; set; }
        public string TimeZone { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string PayPalTransactionId { get; set; }
    }
}
