namespace EzBob.Backend.Models {
	using Ezbob.Utils;

	public enum ConfigTableType
	{
		LoanOfferMultiplier,
		BasicInterestRate,
		EuLoanMonthlyInterest,
		DefaultRateCompany,
		DefaultRateCustomer
	}

	public class ConfigTable : ITraversable
	{
		public int Id { get; set; }
		
		[Traversable]
		public int Start { get; set; }

		[Traversable]
		public int End { get; set; }

		[Traversable]
		public decimal Value { get; set; }
	}
}