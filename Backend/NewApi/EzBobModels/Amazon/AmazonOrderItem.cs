namespace EzBobModels.Amazon {
    using System;

    public class AmazonOrderItem {
        public int Id { get; set; }
        public int? AmazonOrderId { get; set; }
        public string OrderId { get; set; }
        public string SellerOrderId { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string OrderStatus { get; set; }
        public string OrderTotalCurrency { get; set; }
        public decimal? OrderTotal { get; set; }
        public int? NumberOfItemsShipped { get; set; }
        public int? NumberOfItemsUnshipped { get; set; }
    }
}
