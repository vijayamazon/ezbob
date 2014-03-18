namespace Ezbob.Backend.Models
{
	public class MpsTotals
	{
		public double TotalSumOfOrders1YTotal { get; set; }
		public double TotalSumOfOrders3MTotal { get; set; }
		public int MarketplaceSeniorityDays { get; set; }
		public decimal TotalSumOfOrdersForLoanOffer { get; set; }
		public double TotalSumOfOrders1YTotalForRejection { get; set; }
		public double TotalSumOfOrders3MTotalForRejection { get; set; }
	}
}
