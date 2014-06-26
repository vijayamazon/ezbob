namespace Ezbob.Backend.Models
{
	using System;

	public class AffordabilityModel
	{
		public AffordabilityType Type { get; set; }
		public DateTime DateFrom { get; set; }
		public DateTime DateTo { get; set; }
		public decimal Revenues { get; set; }
		public decimal? Opex { get; set; }
		public decimal? ValueAdded { get; set; }
		public decimal? Salaries { get; set; }
		public decimal? Tax { get; set; }
		public decimal? Ebitda { get; set; }
		public decimal? LoanRepayment { get; set; }
		public decimal? Fcf { get; set; }
		public bool HasFullYearData { get; set; }
		// TODO: annualized trend
		// TODO: quarter trend
	}

	public enum AffordabilityType
	{
		HMRC,
		Bank,
		PSP,
		Ecomm,
		Accounting
	}
}