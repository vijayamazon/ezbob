namespace Ezbob.Backend.Strategies.Lottery.FitConditions {
	internal abstract class AAmount : ABase {
		public override void Init(
			int actualLoanCount,
			int? configuredLoanCount,
			decimal actualLoanAmount,
			decimal? configuredLoanAmount
		) {
			ActualLoanAmount = actualLoanAmount;
			ConfiguredLoanAmount = configuredLoanAmount;
		} // Init

		public override bool Calculate() {
			return (null != ConfiguredLoanAmount) && (0 <= ConfiguredLoanAmount) && Fits();
		} // Calculate

		protected abstract bool Fits();

		protected virtual decimal ActualLoanAmount { get; set; }
		protected virtual decimal? ConfiguredLoanAmount { get; set; } 
	} // class MaxCount
} // namespace
