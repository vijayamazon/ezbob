namespace EzBobModels.PayPal {
    using System;

    public class PayPalTransactionItem {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public DateTime? Created { get; set; }
        public int? CurrencyId { get; set; }
        public decimal? FeeAmount { get; set; }
        public decimal? GrossAmount { get; set; }
        public decimal? NetAmount { get; set; }
        public string TimeZone { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string PayPalTransactionId { get; set; }
    }
}
