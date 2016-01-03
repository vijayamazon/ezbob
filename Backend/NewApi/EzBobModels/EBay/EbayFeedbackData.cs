using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.EBay
{
    /// <summary>
    /// DTO for MP_EbayFeedback
    /// </summary>
    public class EbayFeedbackData
    {
        public int Id { get; set; }
        public int CustomerMarketPlaceId { get; set; }
        public DateTime Created { get; set; }
        public int? RepeatBuyerCount { get; set; }
        public double? RepeatBuyerPercent { get; set; }
        public double? TransactionPercent { get; set; }
        public int? UniqueBuyerCount { get; set; }
        public int? UniqueNegativeCount { get; set; }
        public int? UniquePositiveCount { get; set; }
        public int? UniqueNeutralCount { get; set; }
        public int? CustomerMarketPlaceUpdatingHistoryRecordId { get; set; }
    }
}
