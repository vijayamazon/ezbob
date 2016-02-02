namespace EzBobModels.Amazon {
    using System;

    public class AmazonFeedback {
        public int Id { get; set; }
        public int CustomerMarketPlaceId { get; set; }
        public DateTime Created { get; set; }
        public double? UserRating { get; set; }
        public int? CustomerMarketPlaceUpdatingHistoryRecordId { get; set; }
    }
}
