namespace EzBob.Web.Areas.Underwriter.Models
{
    public class ManualPaymentModel
    {
        public string PaymentDate { get; set; }
        public string Description { get; set; }
        public decimal? Fees { get; set; }
        public decimal? Interest { get; set; }
        public string PaymentMethod { get; set; }
		public string WriteOffReason { get; set; }
        public bool SendEmail { get; set; }
        public bool ChargeClient { get; set; }
        public decimal? Principle { get; set; }
        public decimal TotalSumPaid { get; set; }
        public int CustomerId { get; set; }
        public int LoanId { get; set; }
    }

    public class LoanPaymentDetails
    {
        public decimal Fee { get; set; }
        public decimal Interest { get; set; }
        public decimal Principal { get; set; }
        public decimal Balance { get; set; }
        public decimal MinValue { get; set; }
        public decimal Amount { get; set; }
    }
}