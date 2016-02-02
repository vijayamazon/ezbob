namespace EzBobModels.Amazon {
    public class AmazonOrderItemPayment {
        public int Id { get; set; }
        public int? OrderItemId { get; set; }
        public string SubPaymentMethod { get; set; }
        public string MoneyInfoCurrency { get; set; }
        public decimal? MoneyInfoAmount { get; set; }
    }
}
