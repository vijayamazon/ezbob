using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.EBay {
    /// <summary>
    /// DTO for MP_EbayFeedbackItem
    /// </summary>
    public class EbayFeedbackItem {
        public int Id { get; set; }
        public int EbayFeedbackId { get; set; }
        public EbayTimePeriod? TimePeriodId { get; set; }
        public int? Negative { get; set; }
        public int? Positive { get; set; }
        public int? Neutral { get; set; }
    }
}
