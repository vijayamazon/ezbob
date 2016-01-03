using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.EBay {
    public class EbayTransaction {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? QuantityPurchased { get; set; }
        public string PaymentHoldStatus { get; set; }
        public string PaymentMethodUsed { get; set; }
        public double? Price { get; set; }
        public string PriceCurrency { get; set; }
        public string ItemId { get; set; }
        public string ItemPrivateNotes { get; set; }
        public string ItemSellerInventoryID { get; set; }
        public string ItemSKU { get; set; }
        public string eBayTransactionId { get; set; }
        public int? ItemInfoId { get; set; }
    }
}
