using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.EBay {
    /// <summary>
    /// DTO for MP_EbayExternalTransaction
    /// </summary>
    public class EbayExternalTransaction {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public string TransactionID { get; set; }
        public DateTime? TransactionTime { get; set; }
        public string FeeOrCreditCurrency { get; set; }
        public double? FeeOrCreditPrice { get; set; }
        public string PaymentOrRefundACurrency { get; set; }
        public double? PaymentOrRefundAPrice { get; set; }
    }
}
