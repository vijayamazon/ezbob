namespace EzBobModels.Amazon {
    public class AmazonOrderItemDetail {
        public int Id { get; set; }
        public int? OrderItemId { get; set; }
        public string SellerSKU { get; set; }
        public string AmazonOrderItemId { get; set; }
        public string ASIN { get; set; }
        public string CODFeeCurrency { get; set; }
        public decimal? CODFeePrice { get; set; }
        public string CODFeeDiscountCurrency { get; set; }
        public decimal? CODFeeDiscountPrice { get; set; }
        public string GiftMessageText { get; set; }
        public string GiftWrapLevel { get; set; }
        public string GiftWrapPriceCurrency { get; set; }
        public decimal? GiftWrapPrice { get; set; }
        public string GiftWrapTaxCurrency { get; set; }
        public decimal? GiftWrapTaxPrice { get; set; }
        public string ItemPriceCurrency { get; set; }
        public decimal? ItemPrice { get; set; }
        public string ItemTaxCurrency { get; set; }
        public decimal? ItemTaxPrice { get; set; }
        public string PromotionDiscountCurrency { get; set; }
        public decimal? PromotionDiscountPrice { get; set; }
        public int? QuantityOrdered { get; set; }
        public int? QuantityShipped { get; set; }
        public string ShippingDiscountCurrency { get; set; }
        public decimal? ShippingDiscountPrice { get; set; }
        public string ShippingPriceCurrency { get; set; }
        public decimal? ShippingPrice { get; set; }
        public string ShippingTaxCurrency { get; set; }
        public decimal? ShippingTaxPrice { get; set; }
        public string Title { get; set; }
    }
}
