namespace EzBob.Backend.Models {
	using Ezbob.Utils;

	public class LoanOfferMultiplier : ITraversable
	{
		public int Id { get; set; }
		
		[Traversable]
		public int StartScore { get; set; }

		[Traversable]
		public int EndScore { get; set; }

		[Traversable]
		public decimal Multiplier { get; set; }
	} // LoanOfferMultiplier
} // namespace