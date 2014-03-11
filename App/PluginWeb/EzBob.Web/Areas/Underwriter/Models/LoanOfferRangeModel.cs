namespace EzBob.Web.Areas.Underwriter.Models
{
	public class LoanOfferRangeModel
	{
		public int Id { get; set; }
		public int StartValue { get; set; }
		public int EndValue { get; set; }
		public decimal Interest { get; set; }
	}
}