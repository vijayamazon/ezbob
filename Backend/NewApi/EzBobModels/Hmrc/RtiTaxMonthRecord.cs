using System;

namespace EzBobModels.Hmrc
{
    public class RtiTaxMonthRecord
    {
        public int Id { get; set; }
        public int CustomerMarketPlaceId { get; set; }
        public DateTime Created { get; set; }
        public int? CustomerMarketPlaceUpdatingHistoryRecordId { get; set; }
        public int SourceID { get; set; }
    }
}
