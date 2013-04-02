namespace EzBob.Web.Areas.Customer.Models
{
    public class PayPalModel : SimpleMarketPlaceModel
    {
        public int id { get; set; }
        public double TranzactionsNumber { get; set; }
        public double TotalNetInPayments { get; set; }
        public double TotalNetOutPayments { get; set; }
        public string Seniority { get; set; }
        public string Status { get; set; }
    }
}