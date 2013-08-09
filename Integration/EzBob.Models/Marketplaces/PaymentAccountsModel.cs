namespace EzBob.Web.Areas.Customer.Models
{
    public class PaymentAccountsModel : SimpleMarketPlaceModel
    {
        public int id { get; set; }
        public double TransactionsNumber { get; set; }
        public double TotalNetInPayments { get; set; }
		public double MonthInPayments { get; set; }
        public double TotalNetOutPayments { get; set; }
        public string Status { get; set; }
    }
}