namespace EzBob.Web.Areas.Underwriter.Models
{
	public class LoanOfferRangeModel
	{
		public int Id { get; set; }
		public int FromScore { get; set; }
		public int ToScore { get; set; }
		public decimal LoanInterestBase { get; set; }
	}
}