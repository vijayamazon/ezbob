namespace EzBobModels.MarketPlace {
    using System;

    /// <summary>
    /// Represents CustomerMarketPlace taken from MP_CustomerMarketPlace
    /// </summary>
    public class CustomerMarketPlace {
        public int Id { get; set; }
        public int MarketPlaceId { get; set; }
        public int CustomerId { get; set; }
        public byte[] SecurityData { get; set; }
        public string DisplayName { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }
        public DateTime? UpdatingStart { get; set; }
        public DateTime? UpdatingEnd { get; set; }
        public string Warning { get; set; }
        public string UpdateError { get; set; }
        public int? TokenExpired { get; set; }
        public DateTime? OriginationDate { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public bool? Disabled { get; set; }

        [Obsolete("should not be here")]
        public int? AmazonMarketPlaceId { get; set; }

        // UpdatingTimePassInSeconds ---- calculated field
    }
}
