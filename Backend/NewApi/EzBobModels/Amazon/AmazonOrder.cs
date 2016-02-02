namespace EzBobModels.Amazon {
    using System;

    public class AmazonOrder {
        public int Id { get; set; }
        public int CustomerMarketPlaceId { get; set; }
        public DateTime Created { get; set; }
        public int? CustomerMarketPlaceUpdatingHistoryRecordId { get; set; }
    }
}
