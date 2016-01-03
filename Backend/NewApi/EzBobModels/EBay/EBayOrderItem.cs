using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.EBay {
    public class EBayOrderItem {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public double? AdjustmentAmount { get; set; }
        public string AdjustmentCurrency { get; set; }
        public double? AmountPaidAmount { get; set; }
        public string AmountPaidCurrency { get; set; }
        public double? SubTotalAmount { get; set; }
        public string SubTotalCurrency { get; set; }
        public double? TotalAmount { get; set; }
        public string TotalCurrency { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string CheckoutStatus { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentHoldStatus { get; set; }
        public string PaymentMethodsList { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? PaymentTime { get; set; }
        public DateTime? ShippedTime { get; set; }
        public string BuyerName { get; set; }
        public int? ShippingAddressId { get; set; }
    }
}
