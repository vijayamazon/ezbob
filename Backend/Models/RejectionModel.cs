namespace Ezbob.Backend.Models
{
	public class RejectionModel
	{
		public int PayPalNumberOfStores { get; set; }
		public decimal PayPalTotalSumOfOrders3M { get; set; }
		public decimal PayPalTotalSumOfOrders1Y { get; set; }
		public int NumOfDefaultAccounts { get; set; }
		public double Yodlee1Y { get; set; }
		public double Yodlee3M { get; set; }
		public decimal Hmrc1Y { get; set; }
		public decimal Hmrc3M { get; set; }
		public bool HasYodlee { get; set; }
		public bool HasHmrc { get; set; }
		public int CompanyScore { get; set; }
		public int LateAccounts { get; set; }
	}
}
