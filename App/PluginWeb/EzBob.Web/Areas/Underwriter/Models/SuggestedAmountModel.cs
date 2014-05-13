namespace EzBob.Web.Areas.Underwriter.Models {
	using System.ComponentModel;

	public enum CalculationMethod
	{
		[Description("FCF")]
		FCF,
		[Description("Turnover")]
		Turnover,
		[Description("Value added")]
		ValueAdded
	}

	public class SuggestedAmountModel {
		public int Id { get; set; }
		public string Method { get; set; }
		public decimal Silver { get; set; }
		public decimal Gold { get; set; }
		public decimal Diamond { get; set; }
		public decimal Platinum { get; set; }
		public decimal Value { get; set; }
		public SuggestedAmountModel()
		{
			Id = -1;
		} // default constructor
	} // class LoanSourceModel
} // namespace