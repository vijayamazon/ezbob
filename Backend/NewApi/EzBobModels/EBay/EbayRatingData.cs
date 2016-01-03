using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.EBay {
    /// <summary>
    /// DTO for MP_EbayRaitingItem
    /// </summary>
    public class EbayRatingData {
        public int Id { get; set; }
        public int EbayFeedbackId { get; set; }
        public EbayTimePeriod TimePeriodId { get; set; }
        public int? CommunicationCount { get; set; }
        public double? Communication { get; set; }
        public int? ItemAsDescribedCount { get; set; }
        public double? ItemAsDescribed { get; set; }
        public int? ShippingTimeCount { get; set; }
        public double? ShippingTime { get; set; }
        public int? ShippingAndHandlingChargesCount { get; set; }
        public double? ShippingAndHandlingCharges { get; set; }
    }
}
