namespace EzBob.Backend.Models {
	using Ezbob.Utils;

	public class BasicInterestRate : ITraversable {
		public int Id { get; set; }

		[Traversable]
		public int FromScore { get; set; }

		[Traversable]
		public int ToScore { get; set; }

		[Traversable]
		public decimal LoanInterestBase { get; set; }
	} // BasicInterestRate
} // namespace